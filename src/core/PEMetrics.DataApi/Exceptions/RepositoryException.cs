namespace PEMetrics.DataApi.Exceptions;

/// <summary>
/// Exception thrown when a repository operation fails due to a business rule violation.
/// </summary>
public sealed class RepositoryException : Exception
{
    /// <summary>
    /// The SQL Server error code that triggered this exception.
    /// </summary>
    public int ErrorCode { get; }

    public RepositoryException(int errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public RepositoryException(int errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
