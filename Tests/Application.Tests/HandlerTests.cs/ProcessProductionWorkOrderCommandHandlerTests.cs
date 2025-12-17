using Application.Commands;
using Application.Exceptions;
using Application.External;
using Application.Interfaces;
using Application.Policies;
using Domain.ValueObjects;
using Domain.Entities;
using Moq;
using FluentAssertions;
using Domain.Exceptions;
using System.Runtime.InteropServices;

namespace Application.Tests;
public class ProcessProductionWorkOrderCommandHandlerTests
{
    private readonly Mock<IIdempotencyRepository> _idempotencyRepositoryMock;

    private readonly Mock<IWorkOrderRepository> _workOrderRepositoryMock;

    private readonly Mock<IApplicationLogger> _applicationLoggerMock;

    private readonly Mock<IDeadLetterQueue> _deadletterqueueMock;

    private readonly Mock<IInventoryService> _inventoryServiceMock;

    private readonly Mock<IRetryPolicy> _retryPolicyMock;

    private readonly ProcessProductionWorkOrderCommandHandler _handler;

    public ProcessProductionWorkOrderCommandHandlerTests ()
    {
        _idempotencyRepositoryMock = new Mock<IIdempotencyRepository>();

        _workOrderRepositoryMock = new Mock<IWorkOrderRepository>();

        _applicationLoggerMock = new Mock<IApplicationLogger>();

        _deadletterqueueMock = new Mock<IDeadLetterQueue>();

        _inventoryServiceMock = new Mock<IInventoryService>();

        _retryPolicyMock = new Mock<IRetryPolicy>();

        _handler = new ProcessProductionWorkOrderCommandHandler (
            _idempotencyRepositoryMock.Object,
            _workOrderRepositoryMock.Object,
            _deadletterqueueMock.Object,
            _inventoryServiceMock.Object,
            _retryPolicyMock.Object,
            _applicationLoggerMock.Object
            );
    }

    [Fact]
    public async Task Handle_ReturnsDuplicate_WhenIdemKeyExists()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var duplicateCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);

        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(true);

        //Act
        var result = await _handler.Handle(duplicateCommand);

        //Assert
        result.Should().NotBeNull();
        result.Outcome.Should().Be(WorkOrderOutcome.DuplicateIgnored);

    }

    [Fact]

    public async Task Handle_CreatesNewOrder_WhenNoneExists()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);


        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(false);

        _workOrderRepositoryMock
        .Setup(repo => repo.GetByIdAsync(testWorkerId))
        .ReturnsAsync((ProductionWorkOrder?)null);


        //Act
        await _handler.Handle(newCommand);

        //Assert
        _workOrderRepositoryMock
        .Verify ( repo => repo.AddOrderAsync(
            It.Is<ProductionWorkOrder> ( w => 
            w.Id == testWorkerId &&
            w.IdemKey == testKey &&
            w.Payload.ProductId == testProductId &&
            w.Payload.Quantity == 10
            )
        ),
        Times.Once);
    }

    [Fact]

    public async Task Handle_MarksOrderProcessing_BeforeInventoryCheck()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);
        var existingOrder= new ProductionWorkOrder(testWorkerId, testKey, new Payload(testProductId, 10));


        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(false);

        _workOrderRepositoryMock
        .Setup(repo => repo.GetByIdAsync(testWorkerId))
        .ReturnsAsync(existingOrder);

        var observedStates = new List<ProductionWorkOrder.WorkOrderState>();

        _workOrderRepositoryMock
        .Setup(repo => repo.UpdateAsync(It.IsAny<ProductionWorkOrder>()))
        .Callback<ProductionWorkOrder>(w => observedStates.Add(w.State))
        .ReturnsAsync((ProductionWorkOrder w) => w) ;

        //Act
        await _handler.Handle(newCommand);

        //Assert
        observedStates.Should().Contain(ProductionWorkOrder.WorkOrderState.Processing);
    }

    [Fact]

    public async Task Handle_CompletesOrder_WhenInventoryCheckSucceeds ()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);
        var existingOrder= new ProductionWorkOrder(testWorkerId, testKey, new Payload(testProductId, 10));
        var mockInventoryResponse= new InventoryServiceResponse(true, 10, DateTime.UtcNow);

        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(false);

        _workOrderRepositoryMock
        .Setup(repo => repo.GetByIdAsync(testWorkerId))
        .ReturnsAsync(existingOrder);

        _inventoryServiceMock
        .Setup(service => service.CheckAvailabilityAsync(existingOrder.Payload))
        .ReturnsAsync(mockInventoryResponse);

        

        var observedStates = new List<ProductionWorkOrder.WorkOrderState>();

        _workOrderRepositoryMock
        .Setup(repo => repo.UpdateAsync(It.IsAny<ProductionWorkOrder>()))
        .Callback<ProductionWorkOrder>(w => observedStates.Add(w.State))
        .ReturnsAsync((ProductionWorkOrder w) => w) ;

        //Act
        await _handler.Handle(newCommand);


        //Assert
        observedStates.Should().Contain(ProductionWorkOrder.WorkOrderState.Completed);
        
        _idempotencyRepositoryMock
        .Verify(repo => repo.RecordAsync(
            It.Is<IdempotencyKey>(k => k == testKey)
        ),
        Times.Once);
        
    }

    [Fact]

    public async Task Handle_SendsToDLQ_WhenInvalidOrder()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);
        var existingOrder= new ProductionWorkOrder(testWorkerId, testKey, new Payload(testProductId, 10));
        var mockInventoryResponse= new InventoryServiceResponse(true, 10, DateTime.UtcNow);

        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(false);

        _workOrderRepositoryMock
        .Setup(repo => repo.GetByIdAsync(testWorkerId))
        .ReturnsAsync(existingOrder);

        _retryPolicyMock
        .Setup(policy => policy.ExecuteAsync(It.IsAny<Func<Task>>()))
        .ThrowsAsync(new InvalidWorkOrderException("Order invalid"));

        //Act

        await _handler.Handle(newCommand);

        //Assert

        _applicationLoggerMock
        .Verify(log => log.LogErrorAsync(
        "Work order sent to DLQ",
        testWorkerId,
        ProductionWorkOrder.WorkOrderState.Failed,
        It.IsAny<string>()
        ), Times.Once);

        _deadletterqueueMock
        .Verify(queue => queue.EnqueueAsync(
            It.Is<ProductionWorkOrder>(w => 
            w.Id == testWorkerId &&
            w.IdemKey == testKey &&
            w.Payload.ProductId == testProductId &&
            w.Payload.Quantity == 10
            )
        ),
        Times.Once);

    }

    [Fact]

    public async Task Handler_LogsWarnings_WhenExternalServiceUnavailable()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);
        var existingOrder= new ProductionWorkOrder(testWorkerId, testKey, new Payload(testProductId, 10));
        var mockInventoryResponse= new InventoryServiceResponse(true, 10, DateTime.UtcNow);

        _idempotencyRepositoryMock
        .Setup( repo => repo.ExistsAsync(testKey))
        .ReturnsAsync(false);

        _workOrderRepositoryMock
        .Setup(repo => repo.GetByIdAsync(testWorkerId))
        .ReturnsAsync(existingOrder);

        _retryPolicyMock
        .Setup(policy => policy.ExecuteAsync(It.IsAny<Func<Task>>()))
        .ThrowsAsync(new RetryLimitExceededException("Service unavailable"));

        //Act
        await _handler.Handle(newCommand);

        //Assert

        _applicationLoggerMock
        .Verify(log => log.LogErrorAsync(
        "Work order sent to DLQ",
        testWorkerId,
        ProductionWorkOrder.WorkOrderState.Failed,
        It.IsAny<string>()
        ), Times.Once);

        _deadletterqueueMock
        .Verify(queue => queue.EnqueueAsync(
            It.Is<ProductionWorkOrder>(w => 
            w.Id == testWorkerId &&
            w.IdemKey == testKey &&
            w.Payload.ProductId == testProductId &&
            w.Payload.Quantity == 10
            )
        ),
        Times.Once);

        _applicationLoggerMock
        .Verify(log => log.LogWarningAsync(
        "Inventory service unavailable",
        testWorkerId,
        It.IsAny<string>()
        ), Times.Once);
        

    }

 
}