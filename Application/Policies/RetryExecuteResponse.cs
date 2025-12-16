

namespace Application.Policies; 

public record RetryExecuteResponse(
    bool Success,

    int AttemptCount

);