using Application.Interfaces;

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
        return new ProcessProductionWorkOrderResponse(WorkOrderOutcome.Completed);
    }

}