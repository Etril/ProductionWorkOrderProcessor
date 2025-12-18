using Application.Interfaces;
using Domain.Entities;


namespace Infrastructure.Logging; 

public class ApplicationLogger : IApplicationLogger
{
    

    public async Task LogInfoAsync(string message, Guid workOrderId, Guid productId)
    {
        Console.WriteLine($"INFO [{DateTime.UtcNow}] WorkOrderId: {workOrderId}, ProductId: {productId}, Message: {message} ");
    }

    public async Task LogErrorAsync(string message, Guid workOrderId, ProductionWorkOrder.WorkOrderState workOrderState, string exception)
    {
        Console.WriteLine($"ERROR [{DateTime.UtcNow}] WorkOrderId: {workOrderId}, State: {workOrderState}, Error: {exception}");
    }

    public async Task LogWarningAsync (string message, Guid workOrderId, string exceptionMessage)
    {
        Console.WriteLine($"WARNING [{DateTime.UtcNow}] WorkOrderId: {workOrderId}, Warning: {exceptionMessage}");

    }
}