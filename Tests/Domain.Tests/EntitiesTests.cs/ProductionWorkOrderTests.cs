using Domain.Entities;
using Domain.ValueObjects;
using Domain.Exceptions;
using FluentAssertions;
using System.IO.Pipelines;


namespace Domain.Tests; 

public class ProductionWorkOrderTests
{
    [Fact]
    
    public void ProductionWorkOrder_InitialisesAsPending_WhenValid()
    {
        // Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
    

        //Act 
        var result = new ProductionWorkOrder(orderId, idemKey, payload);

        //Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.IdemKey.Should().Be(idemKey);
        result.Payload.Should().Be(payload);
        result.State.Should().Be(ProductionWorkOrder.WorkOrderState.Pending);
    }
    
    [Fact]

    public void ProductionWorkOrder_Throw_WhenNullPayload()
    {

    //Arrange
     var idemKey= new IdempotencyKey();
     var orderId= Guid.NewGuid();

    //Act
    Action act = () => new ProductionWorkOrder(orderId, idemKey, null!);

    //Assert
    act.Should()
    .Throw<ArgumentNullException>();

    }

    [Fact]

    public void MarkProcessing_TransitionsToProcessing_FromPending()
    {

        //Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var pendingOrder= new ProductionWorkOrder(orderId, idemKey, payload);


        //Act
        pendingOrder.MarkProcessing();

        //Assert
        pendingOrder.State.Should().Be(ProductionWorkOrder.WorkOrderState.Processing);
        pendingOrder.Should().NotBeNull();
    }

    [Fact]

    public void MarkProcessing_FromInvalidState_ThrowsInvalidStateTransitionException()
    {
        //Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var processingOrder= new ProductionWorkOrder(orderId, idemKey, payload);
        processingOrder.MarkProcessing();


        //Act
        Action act = () => processingOrder.MarkProcessing();

        //Assert
        act.Should()
        .Throw<InvalidStateTransitionException>()
        .WithMessage("Order has to be pending to be processed");
    }

    [Fact]

    public void MarkCompleted_TransitionsToComplet_FromProcessing()
    {
        //Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var processingOrder= new ProductionWorkOrder(orderId, idemKey, payload);
        processingOrder.MarkProcessing();

        //Act
        processingOrder.MarkCompleted();

        //Assert
        processingOrder.State.Should().Be(ProductionWorkOrder.WorkOrderState.Completed);
        processingOrder.Should().NotBeNull();
    }

    [Fact]

    public void MarkCompleted_FromInvalidState_ThrowsInvalidStateTransitionException()
    {
        //Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var pendingOrder= new ProductionWorkOrder(orderId, idemKey, payload);

        //Act
        Action act = () => pendingOrder.MarkCompleted();

        //Assert
        act.Should()
        .Throw<InvalidStateTransitionException>();
    }

    [Fact]

    public void MarkFailed_TransitionsToFailedAndStoresReason_FromProcessing()
    {
        //Arrange
        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var processingOrder= new ProductionWorkOrder(orderId, idemKey, payload);
        processingOrder.MarkProcessing();

        //Act
        processingOrder.MarkFailed("Service externe indisponible");

        //Assert
        processingOrder.State.Should().Be(ProductionWorkOrder.WorkOrderState.Failed);
        processingOrder.FailureReason.Should().Be("Service externe indisponible");
        
    }

    [Fact]

    public void MarkQuarantined_TransitionsToQuarantinedAndStoresReason_FromFailed()
    {
        //Arrange

        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var idemKey= new IdempotencyKey();
        var orderId= Guid.NewGuid();
        var failedOrder= new ProductionWorkOrder(orderId, idemKey, payload);
        failedOrder.MarkProcessing();
        failedOrder.MarkFailed("Service externe indisponible");

        //Act
        failedOrder.MarkQuarantined("Service externe indisponible");

        //Assert
        failedOrder.State.Should().Be(ProductionWorkOrder.WorkOrderState.Quarantined);
        failedOrder.FailureReason.Should().Be("Service externe indisponible");
    }

    [Fact]

    public void HasSameIdempotencyKey_WithSameValue_ReturnsTrue()
    {
        //Arrange
        var idemKeyValue= new IdempotencyKey();
        var idemKeyOne= idemKeyValue;
        var idemKeyTwo= idemKeyValue;

        var payloadId = Guid.NewGuid();
        var payload = new Payload (payloadId, 10);
        var orderId= Guid.NewGuid();

        var pendingOrder= new ProductionWorkOrder(orderId, idemKeyOne, payload);

        //Act
        var result = pendingOrder.HasSameIdempotencyKey(idemKeyTwo);

        //Assert
        result.Should().BeTrue();
       
    }
}