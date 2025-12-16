

namespace Application.Exceptions; 

public class RetryLimitExceededException : Exception
{
    public RetryLimitExceededException(string message)
    :base (message)
    {
        
    }

}