using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HealthChecks.Kafka
{
    public class KafkaHealthCheck : IHealthCheck
    {
        private readonly ProducerConfig _configuration;
        private readonly string _topic;
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaHealthCheck>? _logger;

        public KafkaHealthCheck(ProducerConfig configuration, string? topic, ILogger<KafkaHealthCheck>? logger = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _topic = topic ?? "healthchecks-topic";
            _logger = logger;
            _producer = ProducerBuilderFactory().Build();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {

                var message = new Message<string, string>
                {
                    Key = "healthcheck-key",
                    Value = $"Check Kafka healthy on {DateTime.UtcNow}"
                };

                _logger?.LogInformation("Using topic: {topic} to send healthcheck message", _topic);
                var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

                if (result.Status == PersistenceStatus.NotPersisted)
                {
                    _logger?.LogWarning("Healthcheck message was not persisted or a failure is raised on health check for kafka.");
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Message is not persisted or a failure is raised on health check for kafka.");
                }

                _logger?.LogInformation("Healthcheck message was persisted Using topic");
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                _logger?.LogError("Exception occurred message: {message}", ex.Message);
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private ProducerBuilder<string, string> ProducerBuilderFactory()
        {
            var producerBuilder = new ProducerBuilder<string, string>(_configuration);
            if (_logger != null)
            {
                _logger.LogInformation("Setting up KafkaLogger for the producer");
                var kafkaLogger = new KafkaLogger(_logger);
                producerBuilder.SetLogHandler(kafkaLogger.Handler);
            }
            return producerBuilder;
        }
    }
}
