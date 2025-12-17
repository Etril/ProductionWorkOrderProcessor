using Domain.ValueObjects;
using Application.External;

namespace Application.Interfaces;

public interface IInventoryService
{
    Task <InventoryServiceResponse> CheckAvailabilityAsync(Payload payload);
}