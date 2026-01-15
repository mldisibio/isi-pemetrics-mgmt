namespace PEMetrics.DataCache.Infrastructure;

/// <summary>Tracks table population state and provides semaphores for blocking queries during population.</summary>
public sealed class TablePopulationTracker
{
    readonly Dictionary<string, SemaphoreSlim> _semaphores = new(StringComparer.OrdinalIgnoreCase);
    readonly object _lock = new();

    static readonly string[] AllTables =
    [
        "Cell",
        "PCStation",
        "CellByPCStation",
        "SwTestMap",
        "CellBySwTest",
        "CellBySwTestView",
        "TLA",
        "CellByPartNo",
        "CellByPartNoView"
    ];

    public TablePopulationTracker()
    {
        foreach (var table in AllTables)
        {
            _semaphores[table] = new SemaphoreSlim(1, 1);
        }
    }

    /// <summary>Acquires the semaphore for a table. Returns a disposable to release when done.</summary>
    public async Task<IDisposable> AcquireAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var semaphore = GetSemaphore(tableName);
        await semaphore.WaitAsync(cancellationToken);
        return new SemaphoreReleaser(semaphore);
    }

    /// <summary>Waits until a table's population is complete (semaphore is available).</summary>
    public async Task WaitForTableAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var semaphore = GetSemaphore(tableName);
        await semaphore.WaitAsync(cancellationToken);
        semaphore.Release();
    }

    /// <summary>Waits until multiple tables' populations are complete.</summary>
    public async Task WaitForTablesAsync(IEnumerable<string> tableNames, CancellationToken cancellationToken = default)
    {
        foreach (var tableName in tableNames)
        {
            await WaitForTableAsync(tableName, cancellationToken);
        }
    }

    SemaphoreSlim GetSemaphore(string tableName)
    {
        lock (_lock)
        {
            if (!_semaphores.TryGetValue(tableName, out var semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                _semaphores[tableName] = semaphore;
            }
            return semaphore;
        }
    }

    sealed class SemaphoreReleaser : IDisposable
    {
        readonly SemaphoreSlim _semaphore;
        bool _disposed;

        public SemaphoreReleaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _semaphore.Release();
        }
    }
}
