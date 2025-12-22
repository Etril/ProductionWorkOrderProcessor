using Domain.Entities;
using Domain.ValueObjects;
using Application.External;
using Application.Commands;
using Application.Policies;
using Infrastructure.Persistence;
using Infrastructure.DLQ;
using Infrastructure.External;
using Infrastructure.Logging;
using Infrastructure.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Tests.Integration; 

public class IntegrationTests
{
    private readonly ProcessProductionWorkOrderCommandHandler _handler;

    public IntegrationTests ()
    {

        var context = TestDbContextFactory.Create(); 
        var idempotencyRepository= new IdempotencyRepository(context);
        var workOrderRepository = new WorkOrderRepository(context);
        var deadLetterQueue = new DeadLetterQueue(context);
        var inventoryService = new FakeInventoryService();

        var delays = new []
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero,
        };

        var retryPolicy = new RetryPolicy(delays);

        var applicationLogger = new ApplicationLogger();

        
        
        _handler = new ProcessProductionWorkOrderCommandHandler(
            idempotencyRepository, 
            workOrderRepository, 
            deadLetterQueue, 
            inventoryService,
            retryPolicy,
            applicationLogger);
    }

    [Fact]

    public async Task ProcessWorkOrder_EndtoEnd_SucceedsWhenValid()
    {
        //Arrange
        var testWorkerId = Guid.NewGuid();
        var testProductId= Guid.NewGuid();
        var testKey = new IdempotencyKey();

        var newCommand= new ProcessProductionWorkOrderCommand(testWorkerId, testProductId, 10, testKey);

        //Act
        var result= await _handler.Handle(newCommand);

        //Assert
        result.Should().NotBeNull();
        result.Outcome.Should().Be(WorkOrderOutcome.Completed);


    }
}