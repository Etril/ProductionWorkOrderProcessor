using Application.Policies;

namespace Application.Interfaces;

public interface IRetryPolicy
{
    Task ExecuteAsync(Func<Task> executeTask);
}