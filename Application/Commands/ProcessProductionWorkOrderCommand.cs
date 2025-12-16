using Domain.ValueObjects;

public record ProcessProductionWorkOrderCommand(
    Guid WorkOrderId, 

    Guid ProductId,

    decimal Quantity, 

    IdempotencyKey IdempotencyKey
);

public record ProcessProductionWorkOrderResponse (
  WorkOrderOutcome Outcome,
    string? Reason = null
);

public enum WorkOrderOutcome
{
    Completed,
    DuplicateIgnored,
    Quarantined,
    Failed
}

