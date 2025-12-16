using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<bool> ExistsAsync(IdempotencyKey idempotencyKey);

    Task RecordAsync (IdempotencyKey idempotencyKey);
}