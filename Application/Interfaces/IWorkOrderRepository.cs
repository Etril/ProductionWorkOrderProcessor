using Domain.Entities;

namespace Application.Interfaces;

public interface IWorkOrderRepository
{
    Task<ProductionWorkOrder?> GetByIdAsync(Guid workOrderId);

    Task AddOrderAsync (ProductionWorkOrder workOrder);

    Task<ProductionWorkOrder> UpdateAsync(ProductionWorkOrder workOrder);
}