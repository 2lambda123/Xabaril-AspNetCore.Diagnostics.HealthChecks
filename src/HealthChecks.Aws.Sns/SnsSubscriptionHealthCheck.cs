using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns
{
    public class SnsSubscriptionHealthCheck : IHealthCheck
    {
        private readonly SnsOptions _snsOptions;

        public SnsSubscriptionHealthCheck(SnsOptions snsOptions)
        {
            _snsOptions = snsOptions ?? throw new ArgumentNullException(nameof(SnsOptions));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = CreateSnsClient();

                foreach (var (topicName, subscriptions) in _snsOptions.TopicsAndSubscriptions.Select(x => (x.Key, x.Value)))
                {
                    var topic = await client.FindTopicAsync(topicName); // <--- want to pass CancellationToken here

                    if (topic == null)
                    {
                        throw new NotFoundException($"Topic {topicName} does not exist");
                    }

                    var attrs = await client.GetTopicAttributesAsync(topic.TopicArn, cancellationToken);

                    if (!attrs.Attributes.TryGetValue("DisplayName", out var displayName))
                    {
                        displayName = "";
                    }

                    var subscriptionsFromAws = await client.ListSubscriptionsByTopicAsync(topicName, cancellationToken);

                    foreach (var subs in subscriptionsFromAws.Subscriptions)
                    {
                        if (!subscriptions.Contains(subs.SubscriptionArn))
                        {

                        }
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }

        private AmazonSimpleNotificationServiceClient CreateSnsClient()
        {
            var credentialsProvided = _snsOptions.Credentials is not null;
            var regionProvided = _snsOptions.RegionEndpoint is not null;
            return (credentialsProvided, regionProvided) switch
            {
                (false, false) => new AmazonSimpleNotificationServiceClient(),
                (false, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.RegionEndpoint),
                (true, false) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials),
                (true, true) => new AmazonSimpleNotificationServiceClient(_snsOptions.Credentials, _snsOptions.RegionEndpoint)
            };
        }
    }
}
