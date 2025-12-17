

using Application.Exceptions;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.Policies;

public class RetryPolicy : IRetryPolicy
{

    IReadOnlyList<TimeSpan> _delays;
    public RetryPolicy(IReadOnlyList<TimeSpan> delays)
    {
        _delays = delays;
    }
    public async Task ExecuteAsync(Func<Task> executeTask)
    {


        for (int attempt = 0; attempt <= _delays.Count; attempt++)
        {
            try
            {
                await executeTask();
                return;

            }
            catch (InvalidWorkOrderException)
            {
                throw;
            }

            catch (Exception ex)
            {
                if (attempt == _delays.Count)
                {
                    throw new RetryLimitExceededException($"Retry limit exceeded with error : {ex}");
                }

                var delay= _delays[attempt];
                await Task.Delay(delay);

            }

        }
    }
}