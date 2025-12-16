
namespace Infrastructure.External;

public record InventoryServiceResponse(
    bool isAvailable, 

    decimal AvailableQuantity, 

    DateTime CheckedAt

);