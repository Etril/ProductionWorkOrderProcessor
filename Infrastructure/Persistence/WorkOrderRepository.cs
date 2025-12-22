using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly AppDbContext _context;

    public WorkOrderRepository (AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductionWorkOrder?> GetByIdAsync(Guid workOrderId)
    {
        return await _context.WorkOrders
        .FirstOrDefaultAsync(w => w.Id == workOrderId);
    }

    public async Task AddOrderAsync(ProductionWorkOrder workOrder)
    {
        await _context.WorkOrders
        .AddAsync(workOrder);
        await _context.SaveChangesAsync();
    }

    public async Task<ProductionWorkOrder> UpdateAsync (ProductionWorkOrder workOrder)
    {
         await _context.SaveChangesAsync();
         return workOrder;
    }
}