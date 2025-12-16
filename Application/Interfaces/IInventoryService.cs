using Domain.ValueObjects;
using Infrastructure.External;

namespace Application.Interfaces;

public interface IInventoryService
{
    Task <InventoryServiceResponse> CheckAvailabilityAsync(Payload payload);
}