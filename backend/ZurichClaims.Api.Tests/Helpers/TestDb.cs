using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZurichClaims.Api.Data;

namespace ZurichClaims.Api.Tests.Helpers;

public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ClaimsDbContext> _options;
    private bool _disposed;

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ClaimsDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var initialContext = CreateDbContext();
        initialContext.Database.Migrate();
    }

    public ClaimsDbContext CreateDbContext()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new ClaimsDbContext(_options);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _connection.Dispose();
        _disposed = true;
    }
}

