namespace HealthChecks.AzureStorage;

/// <summary>
/// Represents a collection of settings that configure Azure Storage Queue Service health checks.
/// </summary>
public sealed class QueueStorageHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the name of the Azure Queue whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>, then no health check is performed for a specific queue.
    /// </remarks>
    /// <value>An optional Azure Queue name.</value>
    public string? QueueName { get; set; }
}
