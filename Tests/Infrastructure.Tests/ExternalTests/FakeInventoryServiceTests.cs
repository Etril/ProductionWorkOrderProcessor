using Domain.Entities;
using Application.External;
using Infrastructure.External;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;
using Domain.Exceptions;


namespace Infrastructure.Tests;

public class FakeInventoryServiceTest
{
    private readonly FakeInventoryService _fakeinventoryService;

    public FakeInventoryServiceTest()
    {
        _fakeinventoryService= new FakeInventoryService();
    }


    [Fact]

    public async Task FakeInventoryService_ThrowsUnavailable_WhenFittingInput()
    {
        //Arrange
        var payload = new Payload(Guid.NewGuid(), 500000);

        //Act 
        Func<Task> act = async () => await _fakeinventoryService.CheckAvailabilityAsync(payload);

        //Assert
        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();

    }

    [Fact]

    public async Task FakeInventoryService_ReturnsSuccess_WhenValid()
    {
        //Arrange
        var payload = new Payload(Guid.NewGuid(), 2000);

        //Act
        var result= await _fakeinventoryService.CheckAvailabilityAsync(payload);

        //Assert
        result.Should().NotBeNull();
        result.isAvailable.Should().Be(true);
        result.AvailableQuantity.Should().Be(payload.Quantity);
    }
}