using Application.Interfaces;
using Application.External;
using Domain.Entities;
using Domain.ValueObjects;
using Domain.Exceptions;


namespace Infrastructure.External;

public class FakeInventoryService : IInventoryService
{

    public async Task<InventoryServiceResponse> CheckAvailabilityAsync (Payload payload)
    {
        if (payload.Quantity <= 0) 
        throw new InvalidWorkOrderException ("Quantity cannot be 0 or negative");

        if (payload.Quantity > 5000)
        throw new ExternalServiceUnavailableException ("External service not responding");

        return new InventoryServiceResponse(true, payload.Quantity, DateTime.UtcNow);
    }
}