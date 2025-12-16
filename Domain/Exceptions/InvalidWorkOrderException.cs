

namespace Domain.Exceptions;

public class InvalidWorkOrderException : Exception
{
    public InvalidWorkOrderException(string message)
    :base (message)
    {
        
    }
}