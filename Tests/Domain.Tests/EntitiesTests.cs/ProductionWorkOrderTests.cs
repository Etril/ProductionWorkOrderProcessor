using Domain.Entities;
using Domain.ValueObjects;
using Domain.Exceptions;
using FluentAssertions;


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
}