

namespace Domain.Exceptions;

public class DuplicateWorkOrderException : Exception
{
    public DuplicateWorkOrderException(string message)
    :base (message)
    {
        
    }
}