using Worker;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker.Worker>();
builder.Services.AddWorkerServices();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var host = builder.Build();
host.Run();
