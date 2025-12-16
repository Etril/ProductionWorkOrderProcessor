using Application.Policies;

namespace Application.Interfaces;

public interface IRetryPolicy
{
    Task <RetryExecuteResponse> ExecuteAsync(Func<Task> executeTask);
}