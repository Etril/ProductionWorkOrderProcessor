
using Domain.Entities;

namespace Application.Interfaces;

public interface IDeadLetterQueue
{
    Task EnqueueAsync(ProductionWorkOrder workOrder);
}