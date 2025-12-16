

using Domain.Entities;

namespace Application.Interfaces;

public interface IApplicationLogger
{
    Task LogInfoAsync(string message, Guid workOrderId, Guid productId );

    Task LogErrorAsync (string message, Guid workOrderId, ProductionWorkOrder.WorkOrderState workOrderState, string exceptionMessage);

    Task LogWarningAsync (string message, Guid workOrderId, string exceptionMessage);
}