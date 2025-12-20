using Domain.Entities;
using Application.Commands;
using Application.Exceptions;
using Application.Interfaces;
using Application.Policies;
using Infrastructure.Persistence;

namespace Infrastructure.DLQ;

public class DeadLetterQueue : IDeadLetterQueue
{
    
    private readonly AppDbContext _context;

    public DeadLetterQueue (AppDbContext context)
    {
        _context = context;
    }


    public async Task EnqueueAsync (ProductionWorkOrder workOrder)
    {
       var record = new DeadLetterRecord
        {
            Id = Guid.NewGuid(),
            WorkOrderId = workOrder.Id,
            FailureReason = workOrder.FailureReason,
            FailedAt = DateTime.UtcNow,
            ProductId = workOrder.Payload.ProductId,
            Quantity = workOrder.Payload.Quantity,
            IdempotencyKey = workOrder.IdemKey.Value
        };

        await _context.DeadLetters.AddAsync(record);
        await _context.SaveChangesAsync();
    }

}