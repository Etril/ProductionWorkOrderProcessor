using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.Commands; 

public class ProcessProductionWorkOrderCommandHandler
{
    private readonly IIdempotencyRepository _idempotencyRepository;

    private readonly IWorkOrderRepository _workOrderRepository;

    private readonly IDeadLetterQueue _deadLetterQueue;

    private readonly IInventoryService _inventoryService;

    private readonly IRetryPolicy _retryPolicy;

    private readonly IApplicationLogger _applicationLogger;

    public ProcessProductionWorkOrderCommandHandler (
        
        IIdempotencyRepository idempotencyRepository,

        IWorkOrderRepository workOrderRepository,

        IDeadLetterQueue deadLetterQueue,

        IInventoryService inventoryService,

        IRetryPolicy retryPolicy, 

        IApplicationLogger applicationLogger
    )

    {
        _idempotencyRepository = idempotencyRepository;

        _workOrderRepository = workOrderRepository;

        _deadLetterQueue = deadLetterQueue;

        _inventoryService = inventoryService;
 
        _retryPolicy = retryPolicy; 

        _applicationLogger = applicationLogger;
    }

    public async Task<ProcessProductionWorkOrderResponse> Handle (ProcessProductionWorkOrderCommand command)
    {
        if (await _idempotencyRepository.ExistsAsync(command.IdempotencyKey))
        {
            await _applicationLogger.LogInfoAsync (
                "Duplicate work order ignored",
                command.WorkOrderId,
                command.ProductId
            );
            return new ProcessProductionWorkOrderResponse(WorkOrderOutcome.DuplicateIgnored);
        }

        var workOrder = await _workOrderRepository.GetByIdAsync(command.WorkOrderId);

        if (workOrder == null)
        {
            workOrder= new ProductionWorkOrder(command.WorkOrderId, command.IdempotencyKey, new Payload(command.ProductId, command.Quantity));
           await _workOrderRepository.AddOrderAsync(workOrder);
        }

        try
        {
            workOrder.MarkProcessing();
            await _workOrderRepository.UpdateAsync(workOrder);
            await _applicationLogger.LogInfoAsync ("New work order created",
            workOrder.Id, workOrder.Payload.ProductId);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _applicationLogger.LogInfoAsync("Checking inventory",
                workOrder.Id,
                workOrder.Payload.ProductId);
                await _inventoryService.CheckAvailabilityAsync(workOrder.Payload);
            } );

            workOrder.MarkCompleted();
            await _workOrderRepository.UpdateAsync(workOrder);

            await _idempotencyRepository.RecordAsync(command.IdempotencyKey);

            await _applicationLogger.LogInfoAsync (
                "Work order processed successfully",
                workOrder.Id,
                workOrder.Payload.ProductId
            );
        }

        catch (InvalidWorkOrderException ex)
        {
            workOrder.MarkFailed("Order was invalid from the start");

            await _workOrderRepository.UpdateAsync(workOrder);

            await _applicationLogger.LogErrorAsync("Work order sent to DLQ",
            workOrder.Id, workOrder.State, ex.Message);

            await _deadLetterQueue.EnqueueAsync(workOrder);

        }

        catch (RetryLimitExceededException ex)
        {
            workOrder.MarkFailed("Retry limit exceeded");
            await _workOrderRepository.UpdateAsync(workOrder);

            await _applicationLogger.LogErrorAsync("Work order sent to DLQ",
            workOrder.Id, workOrder.State, ex.Message);

            await _deadLetterQueue.EnqueueAsync(workOrder);

            await _applicationLogger.LogWarningAsync("Inventory service unavailable",
            workOrder.Id, 
            ex.Message);
        }

        
        
        
        
        return new ProcessProductionWorkOrderResponse(WorkOrderOutcome.Completed);
    }

}