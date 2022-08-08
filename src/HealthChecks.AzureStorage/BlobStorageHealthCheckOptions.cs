namespace HealthChecks.AzureStorage;

/// <summary>
/// Represents a collection of settings that configure Azure Blob Storage health checks.
/// </summary>
public sealed class BlobStorageHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the name of the Azure Storage container whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>, then no health check is performed for a specific container.
    /// </remarks>
    /// <value>An optional Azure Storage container name.</value>
    public string? ContainerName { get; set; }
}
