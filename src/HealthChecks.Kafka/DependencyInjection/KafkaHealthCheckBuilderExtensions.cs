using Confluent.Kafka;
using HealthChecks.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="KafkaHealthCheck"/>.
    /// </summary>
    public static class KafkaHealthCheckBuilderExtensions
    {
        private const string NAME = "kafka";

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="config">The kafka connection configuration parameters to be used.</param>
        /// <param name="topic">The topic name to produce kafka messages on. Optional. If <c>null</c> the topic default 'healthcheck-topic' will be used for the name.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="logger">An optional <see cref="ILogger"/> used to set the LogHandler for the producer .</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddKafka(
            this IHealthChecksBuilder builder,
            ProducerConfig config,
            string? topic = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            ILogger? logger = default)
        {
            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                new KafkaHealthCheck(config, topic, logger),
                failureStatus,
                tags,
                timeout));
        }

        /// <summary>
        /// Add a health check for Kafka cluster.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="setup">The action to configure the kafka connection configuration parameters to be used.</param>
        /// <param name="topic">The topic name to produce kafka messages on. Optional. If <c>null</c> the topic default 'healthcheck-topic' will be used for the name.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'kafka' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <param name="logger">An optional <see cref="ILogger"/> used to set the LogHandler for the producer .</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddKafka(
            this IHealthChecksBuilder builder,
            Action<ProducerConfig> setup,
            string? topic = default,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default,
            ILogger? logger = default)
        {
            var config = new ProducerConfig();
            setup?.Invoke(config);

            return builder.Add(new HealthCheckRegistration(
                name ?? NAME,
                new KafkaHealthCheck(config, topic, logger),
                failureStatus,
                tags,
                timeout));
        }
    }
}
