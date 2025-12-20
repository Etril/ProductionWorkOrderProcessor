using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.DLQ;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext: DbContext
{
    public AppDbContext (DbContextOptions<AppDbContext> options)
    :base(options)
    {
        
    }

    public DbSet<ProductionWorkOrder> WorkOrders { get; set; }
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; }
    public DbSet<DeadLetterRecord> DeadLetters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductionWorkOrder>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.CreatedAt).IsRequired();
            entity.Property(w => w.State)
            .HasConversion<int>()
            .IsRequired();

            entity.OwnsOne(w => w.Payload, payload =>
            {
                payload.Property(p => p.ProductId).IsRequired();
                payload.Property(p => p.Quantity).IsRequired();
            });

            entity.OwnsOne(w => w.IdemKey, idemkey =>
            {
                idemkey.Property(i => i.Value).IsRequired();
            });

        });

        modelBuilder.Entity<DeadLetterRecord>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FailedAt).IsRequired();
            entity.Property(d => d.FailureReason).IsRequired();
            entity.Property(d => d.WorkOrderId).IsRequired();
            entity.Property(d => d.ProductId).IsRequired();
            entity.Property(d => d.Quantity).IsRequired();
            entity.Property(d => d.IdempotencyKey).IsRequired();
            });
        

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(i => i.Value);
        });


    
    }

    
}