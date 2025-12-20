


namespace Infrastructure.DLQ; 

public class DeadLetterRecord
{
    public Guid Id { get; set; }
    public Guid WorkOrderId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime FailedAt { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public Guid IdempotencyKey { get; set; }
}