using System.Collections.ObjectModel;
using EventStore.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.EventStore.gRPC;

/// <summary>
/// Checks whether a gRPC connection can be made to EventStore services using the supplied connection string.
/// </summary>
public class EventStoreHealthCheck : IHealthCheck, IDisposable
{
    private readonly EventStoreClient _client;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "healthcheck.name", nameof(EventStoreHealthCheck) },
                    { "healthcheck.task", "ready" },
                    { "db.system", "azuretable" },
                    { "event.name", "database.healthcheck"}
    };

    public EventStoreHealthCheck(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        _client = new EventStoreClient(EventStoreClientSettings.Create(connectionString));
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
        try
        {
            var readAllStreamResult = _client.ReadAllAsync(
                direction: Direction.Backwards,
                position: Position.End,
                maxCount: 1,
                cancellationToken: cancellationToken);

            await foreach (var _ in readAllStreamResult.Messages.WithCancellation(cancellationToken))
            {
                // If there are messages in the response,
                // that means we successfully connected to EventStore
                return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
            }

            return new HealthCheckResult(context.Registration.FailureStatus, "Failed to connect to EventStore.");
        }
        catch (Exception exception)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: exception, data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
    }

    public virtual void Dispose() => _client.Dispose();
}
