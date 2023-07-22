using System.Collections;
using IBM.WMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IbmMQ;

/// <summary>
/// A health check for IBM MQ services.
/// </summary>
#if NETSTANDARD2_0
[Obsolete("Use .NET6 based MQ Client libraries", false)]
#endif
public class IbmMQHealthCheck : IHealthCheck
{
    private readonly Hashtable _connectionProperties;
    private readonly string _queueManager;

    public IbmMQHealthCheck(string queueManager, Hashtable connectionProperties)
    {
        Guard.ThrowIfNull(queueManager, true);

        _queueManager = queueManager;
        _connectionProperties = Guard.ThrowIfNull(connectionProperties);
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new MQQueueManager(_queueManager, _connectionProperties);
            return HealthCheckResultTask.Healthy;
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: ex));
        }
    }
}
