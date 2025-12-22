using Infrastructure.DLQ;
using Infrastructure.External;
using Infrastructure.Logging;
using Infrastructure.Persistence;
using Application.Policies;
using Application.Interfaces;
using Application.Commands;

namespace Worker.Extensions; 

public static class ServiceExtensions
{
    public static void AddWorkerServices (this IServiceCollection services)
    {
        services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
        services.AddScoped<IIdempotencyRepository, IIdempotencyRepository>();
        services.AddScoped<IDeadLetterQueue, DeadLetterQueue>();
        services.AddScoped<IInventoryService, FakeInventoryService>();
        services.AddScoped<IApplicationLogger, ApplicationLogger>();
        services.AddSingleton<IRetryPolicy>(
            new RetryPolicy(new[]
            {
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
        );
        services.AddScoped<ProcessProductionWorkOrderCommandHandler>();
    }
}