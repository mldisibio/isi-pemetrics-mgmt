namespace PEMetrics.DataApi.Ports;

/// <summary>Port for notifying about data communication errors. Required dependency for all repositories.</summary>
public interface ForNotifyingDataCommunicationErrors
{
    /// <summary>Called when SQL Server is unreachable at startup. Triggers graceful offline mode.</summary>
    void ProductionStoreNotReachable(Exception exception);

    /// <summary>Called for all unexpected errors after startup. Operation describes the failing context.</summary>
    void UnexpectedError(string operation, Exception exception);
}
