using PEMetrics.DataApi.Ports;

namespace PEMetrics.IntegrationTests.Fixtures;

/// <summary>Recording implementation of ForNotifyingDataCommunicationErrors for verifying error handling.</summary>
public sealed class RecordingErrorNotifier : ForNotifyingDataCommunicationErrors
{
    readonly List<(string Method, string? Operation, Exception? Exception)> _calls = [];

    public IReadOnlyList<(string Method, string? Operation, Exception? Exception)> Calls => _calls.AsReadOnly();

    public void Clear() => _calls.Clear();

    public void ProductionStoreNotReachable(Exception exception)
        => _calls.Add((nameof(ProductionStoreNotReachable), null, exception));

    public void UnexpectedError(string operation, Exception exception)
        => _calls.Add((nameof(UnexpectedError), operation, exception));

    // Assertion helpers
    public bool WasCalled(string methodName)
        => _calls.Any(c => c.Method == methodName);

    public bool WasCalledWithOperation(string operation)
        => _calls.Any(c => c.Operation == operation);

    public int CallCount(string methodName)
        => _calls.Count(c => c.Method == methodName);
}
