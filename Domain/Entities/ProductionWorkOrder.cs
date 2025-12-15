using Domain.ValueObjects;

namespace Domain.Entities;

public class ProductionWorkOrder
{
    public Guid Id {get; private set;}

    public IdempotencyKey IdemKey { get; private set; }

    public DateTime CreatedAt {get; private set;}

    public WorkOrderState State   { get; private set;}

    public Payload Payload { get; private set; }

    public ProductionWorkOrder(Guid id, IdempotencyKey idemKey, Payload payload)
    {
        Id = id ;
        IdemKey= idemKey;
        Payload = payload;
        CreatedAt = DateTime.UtcNow;
        State=  WorkOrderState.Pending;
    }

    public enum WorkOrderState
    {
        Pending, 
        Processing,
        Completed,
        Failed,
        Quarantined
    }


}