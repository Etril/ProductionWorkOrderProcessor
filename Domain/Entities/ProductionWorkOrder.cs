using Domain.ValueObjects;
using Domain.Exceptions;

namespace Domain.Entities;

public class ProductionWorkOrder
{
    public Guid Id {get; private set;}

    public IdempotencyKey IdemKey { get; private set; }

    public DateTime CreatedAt {get; private set;}

    public WorkOrderState State   { get; private set;}

    public Payload Payload { get; private set; }

    public string? FailureReason { get; private set; }

    public ProductionWorkOrder(Guid id, IdempotencyKey idemKey, Payload payload)
    {

        if (payload == null) 
        throw new ArgumentNullException();

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

    public void MarkProcessing()
    {
        if (State != WorkOrderState.Pending) 
        throw  new InvalidStateTransitionException("Order has to be pending to be processed");

        State = WorkOrderState.Processing;

    }

    public void MarkCompleted()
    {
        if (State != WorkOrderState.Processing)
        throw new InvalidStateTransitionException("Order has to be processing to be completed");

        State = WorkOrderState.Completed;
    }

    public void MarkFailed(string reason)
    {
        if (State != WorkOrderState.Processing)
        throw new InvalidStateTransitionException("Order needs to be processing to fail");

        FailureReason= reason;
        State = WorkOrderState.Failed;
    }

    public void MarkQuarantined(string reason)
    {
        if (State != WorkOrderState.Failed)
        throw new InvalidStateTransitionException("Order needs to have failed before being quarantined");
        
        FailureReason= reason;
        State= WorkOrderState.Quarantined;
     }

     public bool HasSameIdempotencyKey(IdempotencyKey key)
    {
        return IdemKey.Equals(key);
    }


}