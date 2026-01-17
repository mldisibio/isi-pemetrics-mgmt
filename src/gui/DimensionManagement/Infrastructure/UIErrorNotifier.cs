using PEMetrics.DataApi.Ports;

namespace DimensionManagement.Infrastructure;

/// <summary>UI implementation that displays errors in the status bar.</summary>
public sealed class UIErrorNotifier : ForNotifyingDataCommunicationErrors
{
    readonly Action<string> _showError;
    readonly Action<bool> _setOfflineMode;

    public UIErrorNotifier(Action<string> showError, Action<bool> setOfflineMode)
    {
        _showError = showError ?? throw new ArgumentNullException(nameof(showError));
        _setOfflineMode = setOfflineMode ?? throw new ArgumentNullException(nameof(setOfflineMode));
    }

    public void ProductionStoreNotReachable(Exception exception)
    {
        _setOfflineMode(true);
        _showError($"SQL Server unavailable: {exception.Message}");
    }

    public void UnexpectedError(string operation, Exception exception)
    {
        _showError($"Error in {operation}: {exception.Message}");
    }
}
