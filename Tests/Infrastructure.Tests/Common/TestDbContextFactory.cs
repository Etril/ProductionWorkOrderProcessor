using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using System.ComponentModel.Design.Serialization;

namespace Infrastructure.Tests.Common;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var connection= new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(connection)
        .Options;

        var context= new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}