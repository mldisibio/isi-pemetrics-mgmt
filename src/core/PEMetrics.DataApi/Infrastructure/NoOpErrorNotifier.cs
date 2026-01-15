using PEMetrics.DataApi.Ports;

namespace PEMetrics.DataApi.Infrastructure;

/// <summary>No-op implementation of ForNotifyingDataCommunicationErrors. Use when error notifications are not needed.</summary>
public sealed class NoOpErrorNotifier : ForNotifyingDataCommunicationErrors
{
    public void ProductionStoreNotReachable(Exception exception) { }
    public void UnexpectedError(string operation, Exception exception) { }
}
