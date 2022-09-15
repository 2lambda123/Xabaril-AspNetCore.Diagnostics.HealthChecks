using System.Reflection;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Publisher.CloudWatch
{
    /// <summary>
    /// A health check publisher for AWS CloudWatch.
    /// </summary>
    internal class CloudWatchPublisher : IHealthCheckPublisher, IDisposable
    {
        private readonly List<Dimension> _dimensions;
        private readonly AmazonCloudWatchClient _amazonCloudWatchClient;

        /// <summary>
        /// CloudWatchPublisher constructor
        /// </summary>
        public CloudWatchPublisher()
        {
            _amazonCloudWatchClient = new AmazonCloudWatchClient();

            var serviceCheckName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "undefined";

            _dimensions = new List<Dimension> {
                new Dimension
                {
                    Name = serviceCheckName,
                    Value = serviceCheckName
                }
            };
        }

        /// <summary>
        /// CloudWatchPublisher constructor
        /// </summary>
        /// <param name="serviceCheckName"></param>
        public CloudWatchPublisher(string serviceCheckName) : this()
        {
            if (string.IsNullOrEmpty(serviceCheckName))
                throw new ArgumentNullException(nameof(serviceCheckName));

            _dimensions = new List<Dimension> {
                new Dimension
                {
                    Name = serviceCheckName,
                    Value = serviceCheckName
                }
            };
        }

        /// <summary>
        /// CloudWatchPublisher constructor
        /// </summary>
        /// <param name="region"></param>
        /// <param name="awsAccessKeyId"></param>
        /// <param name="awsSecretAccessKey"></param>
        public CloudWatchPublisher(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region)
        {
            _amazonCloudWatchClient = new AmazonCloudWatchClient(awsAccessKeyId, awsSecretAccessKey, region);

            var serviceCheckName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "undefined";

            _dimensions = new List<Dimension> {
                new Dimension
                {
                    Name = serviceCheckName,
                    Value = serviceCheckName
                }
            };
        }

        /// <summary>
        /// CloudWatchPublisher constructor
        /// </summary>
        /// <param name="serviceCheckName"></param>
        /// <param name="region"></param>
        /// <param name="awsAccessKeyId"></param>
        /// <param name="awsSecretAccessKey"></param>
        public CloudWatchPublisher(string serviceCheckName, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region) : this(awsAccessKeyId, awsSecretAccessKey, region)
        {
            if (string.IsNullOrEmpty(serviceCheckName))
                throw new ArgumentNullException(nameof(serviceCheckName));

            _dimensions = new List<Dimension> {
                new Dimension
                {
                    Name = serviceCheckName,
                    Value = serviceCheckName
                }
            };
        }

        /// <summary>
        /// Publishes the HealthReport to AWS CloudWatch
        /// </summary>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            var putMetricDataRequest = BuildCloudWatchMetricDataRequest(report);

            _amazonCloudWatchClient.PutMetricDataAsync(putMetricDataRequest, cancellationToken);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Builds the CloudWatch MetricDataRequest
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private PutMetricDataRequest BuildCloudWatchMetricDataRequest(HealthReport report)
        {
            var utcNow = DateTime.UtcNow;

            var metricDatas = new List<MetricDatum>
            {
                new MetricDatum
                {
                    Dimensions = _dimensions,
                    MetricName = "status",
                    StatisticValues = new StatisticSet(),
                    TimestampUtc = utcNow,
                    Unit = StandardUnit.Count,
                    Value = (int)report.Status
                }
            };

            var entriesMetricDatas = report.Entries.Select(keyedEntry =>
            {
                return new MetricDatum
                {
                    Dimensions = _dimensions,
                    MetricName = keyedEntry.Key,
                    StatisticValues = new StatisticSet(),
                    TimestampUtc = utcNow,
                    Unit = StandardUnit.Count,
                    Value = (int)keyedEntry.Value.Status
                };
            });

            if (entriesMetricDatas.Any())
                metricDatas.AddRange(entriesMetricDatas);

            return new PutMetricDataRequest
            {
                MetricData = metricDatas,
                Namespace = "Xabaril/AspNetCoreDiagnosticsHealthChecks"
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _amazonCloudWatchClient?.Dispose();
        }

        ~CloudWatchPublisher()
        {
            Dispose(false);
        }
    }
}