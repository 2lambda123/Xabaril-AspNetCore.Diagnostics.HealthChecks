using Azure.Core;
using Azure.Messaging.ServiceBus.Administration;
using HealthChecks.AzureServiceBus.Configuration;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HealthChecks.AzureServiceBus.Tests;

public class azureservicebustopichealthcheck_should
{
    private const string HEALTH_CHECK_NAME = "unit-test-check";

    private readonly string ConnectionString;
    private readonly string FullyQualifiedName;
    private readonly string TopicName;
    private readonly ServiceBusClientProvider _clientProvider;
    private readonly ServiceBusAdministrationClient _serviceBusAdministrationClient;
    private readonly TokenCredential _tokenCredential;

    public azureservicebustopichealthcheck_should()
    {
        ConnectionString = Guid.NewGuid().ToString();
        FullyQualifiedName = Guid.NewGuid().ToString();
        TopicName = Guid.NewGuid().ToString();

        _clientProvider = Substitute.For<ServiceBusClientProvider>();
        _serviceBusAdministrationClient = Substitute.For<ServiceBusAdministrationClient>();
        _tokenCredential = Substitute.For<TokenCredential>();

        _clientProvider.CreateManagementClient(ConnectionString).Returns(_serviceBusAdministrationClient);
        _clientProvider.CreateManagementClient(FullyQualifiedName, _tokenCredential).Returns(_serviceBusAdministrationClient);
    }

    [Fact]
    public async Task can_create_client_with_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, connectionString: ConnectionString);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_connection_string_with_different_topic()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, connectionString: ConnectionString);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var (otherHealthCheck, otherContext) = CreateTopicHealthCheck(otherTopicName, connectionString: ConnectionString);

        var otherActual = await otherHealthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);
    }

    [Fact]
    public async Task can_create_client_with_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, fullyQualifiedName: FullyQualifiedName);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(FullyQualifiedName, _tokenCredential);
    }

    [Fact]
    public async Task reuses_existing_client_when_using_same_fully_qualified_name_with_different_topic()
    {
        // First call
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, fullyQualifiedName: FullyQualifiedName);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        // Second call
        var otherTopicName = Guid.NewGuid().ToString();
        var (otherHealthCheck, otherContext) = CreateTopicHealthCheck(otherTopicName, fullyQualifiedName: FullyQualifiedName);

        var otherActual = await otherHealthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        _clientProvider
            .Received(1)
            .CreateManagementClient(FullyQualifiedName, _tokenCredential);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service_through_administration_and_connection_string()
    {
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, connectionString: ConnectionString);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _clientProvider
            .Received(1)
            .CreateManagementClient(ConnectionString);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_healthy_when_only_checking_healthy_service_through_administration_and_fully_qualified_name()
    {
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, fullyQualifiedName: FullyQualifiedName);

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Healthy);

        _clientProvider
            .Received(1)
            .CreateManagementClient(FullyQualifiedName, _tokenCredential);

        await _serviceBusAdministrationClient
            .Received(1)
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task return_unhealthy_when_exception_is_thrown_by_administration_client()
    {
        using var tokenSource = new CancellationTokenSource();
        var (healthCheck, context) = CreateTopicHealthCheck(TopicName, connectionString: ConnectionString);

        _serviceBusAdministrationClient
            .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var actual = await healthCheck
            .CheckHealthAsync(context, tokenSource.Token)
            .ConfigureAwait(false);

        actual.Status.ShouldBe(HealthStatus.Unhealthy);

        await _serviceBusAdministrationClient
           .Received(1)
           .GetTopicRuntimePropertiesAsync(TopicName, cancellationToken: tokenSource.Token)
           .ConfigureAwait(false);
    }

    private (AzureServiceBusTopicHealthCheck, HealthCheckContext) CreateTopicHealthCheck(
        string topicName,
        string? connectionString = null,
        string? fullyQualifiedName = null)
    {
        var options = new AzureServiceBusTopicHealthCheckOptions(topicName)
        {
            ConnectionString = connectionString,
            FullyQualifiedNamespace = fullyQualifiedName,
            Credential = fullyQualifiedName is null ? null : _tokenCredential,
        };
        var healthCheck = new AzureServiceBusTopicHealthCheck(options, _clientProvider);
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(HEALTH_CHECK_NAME, healthCheck, HealthStatus.Unhealthy, null)
        };
        return (healthCheck, context);
    }
}
