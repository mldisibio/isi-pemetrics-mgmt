using System.Data;
using System.Data.Common;

namespace PEMetrics.DataCache.Infrastructure;

/// <summary>
/// Wraps a DbConnection and prevents disposal. Used to share a single long-lived
/// DuckDB connection across multiple consumers without premature disposal.
/// </summary>
internal sealed class NonDisposingConnectionWrapper : DbConnection
{
    readonly DbConnection _inner;

    public NonDisposingConnectionWrapper(DbConnection inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public override string ConnectionString
    {
        get => _inner.ConnectionString;
#pragma warning disable CS8765 // Nullability of parameter doesn't match overridden member
        set => _inner.ConnectionString = value;
#pragma warning restore CS8765
    }

    public override string Database => _inner.Database;
    public override string DataSource => _inner.DataSource;
    public override string ServerVersion => _inner.ServerVersion;
    public override ConnectionState State => _inner.State;

    public override void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);
    public override void Close() { } // Don't close the underlying connection
    public override void Open() => _inner.Open();
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => _inner.BeginTransaction(isolationLevel);
    protected override DbCommand CreateDbCommand() => _inner.CreateCommand();

    // Don't dispose the underlying connection
    protected override void Dispose(bool disposing) { }
    public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
