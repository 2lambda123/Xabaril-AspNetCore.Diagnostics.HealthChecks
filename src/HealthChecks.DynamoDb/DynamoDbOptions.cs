using Amazon;
using Amazon.Runtime;

namespace HealthChecks.DynamoDb
{
    /// <summary>
    /// Options for <see cref="DynamoDbHealthCheck"/>.
    /// </summary>
    public class DynamoDBOptions
    {
        public AWSCredentials? Credentials { get; set; }

        [Obsolete("Specify the access key and secret as a BasicCredential to the Credentials property instead")]
        public string AccessKey { get; set; } = null!;

        [Obsolete("Specify the access key and secret as a BasicCredential to the Credentials property instead")]
        public string SecretKey { get; set; } = null!;
        public RegionEndpoint RegionEndpoint { get; set; } = null!;
    }
}
