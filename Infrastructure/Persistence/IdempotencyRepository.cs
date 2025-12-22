using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly AppDbContext _context;

    public IdempotencyRepository (AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync (IdempotencyKey idempotencyKey)
    {
      return await _context.IdempotencyRecords
      .AnyAsync(k => k.Value == idempotencyKey.Value);
    }

    public async Task RecordAsync (IdempotencyKey idempotencyKey)
    {
       var record = new IdempotencyRecord
        {
            Value = idempotencyKey.Value
        };

        await _context.IdempotencyRecords.AddAsync(record);
        await _context.SaveChangesAsync();
    }


}