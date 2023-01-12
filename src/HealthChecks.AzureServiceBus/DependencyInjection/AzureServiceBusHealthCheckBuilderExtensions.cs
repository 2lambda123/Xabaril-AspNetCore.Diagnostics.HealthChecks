using Azure.Core;
using Azure.Messaging.EventHubs;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="AzureEventHubHealthCheck"/>,
/// <see cref="AzureServiceBusHealthCheck"/>, <see cref="AzureServiceBusQueueHealthCheck"/>,
/// <see cref="AzureServiceBusSubscriptionHealthCheck"/>, <see cref="AzureServiceBusTopicHealthCheck"/>.
/// </summary>
public static class AzureServiceBusHealthCheckBuilderExtensions
{
    private const string AZUREEVENTHUB_NAME = "azureeventhub";
    private const string AZUREQUEUE_NAME = "azurequeue";
    private const string AZURETOPIC_NAME = "azuretopic";
    private const string AZURESUBSCRIPTION_NAME = "azuresubscription";

    /// <summary>
    /// Add a health check for specified Azure Event Hub.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The azure event hub connection string.</param>
    /// <param name="eventHubName">The azure event hub name.</param>
    /// <param name="configure">An optional action to allow additional Azure Event Hub configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        string connectionString,
        string eventHubName,
        Action<AzureEventHubOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureEventHub(
            _ => connectionString,
            _ => eventHubName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Event Hub.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the azure event hub connection string.</param>
    /// <param name="eventHubNameFactory">A factory to build the azure event hub name.</param>
    /// <param name="configure">An optional action to allow additional Azure Event Hub configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> eventHubNameFactory,
        Action<AzureEventHubOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(eventHubNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREEVENTHUB_NAME,
            sp =>
            {
                var options = new AzureEventHubOptions
                {
                    ConnectionString = connectionStringFactory(sp),
                    EventHubName = eventHubNameFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureEventHubHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Event Hub.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpoint">The azure event hub fully qualified namespace.</param>
    /// <param name="eventHubName">The azure event hub name.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Event Hub configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        string endpoint,
        string eventHubName,
        TokenCredential tokenCredential,
        Action<AzureEventHubOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureEventHub(
            _ => endpoint,
            _ => eventHubName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Event Hub.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpointFactory">A factory to build the azure event hub fully qualified namespace.</param>
    /// <param name="eventHubNameFactory">A factory to build the azure event hub name.</param>
    /// <param name="tokenCredentialFactory">A factory to build The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Event Hub configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> endpointFactory,
        Func<IServiceProvider, string> eventHubNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureEventHubOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(endpointFactory);
        Guard.ThrowIfNull(eventHubNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREEVENTHUB_NAME,
            sp =>
            {
                var options = new AzureEventHubOptions
                {
                    Endpoint = endpointFactory(sp),
                    EventHubName = eventHubNameFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureEventHubHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Event Hub.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="eventHubConnectionFactory">The event hub connection factory used to create a event hub connection for this health check.</param>
    /// <param name="configure">An optional action to allow additional Azure Event Hub configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azureeventhub' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureEventHub(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, EventHubConnection> eventHubConnectionFactory,
        Action<AzureEventHubOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(eventHubConnectionFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREEVENTHUB_NAME,
            sp =>
            {
                var options = new AzureEventHubOptions { Connection = eventHubConnectionFactory(sp) };
                configure?.Invoke(options);
                return new AzureEventHubHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The azure service bus connection string to be used.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        string connectionString,
        string queueName,
        Action<AzureServiceBusQueueOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusQueue(
            _ => connectionString,
            _ => queueName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the azure service bus connection string to be used.</param>
    /// <param name="queueNameFactory">A factory to build the queue name to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Action<AzureServiceBusQueueOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(queueNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueOptions(queueNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpoint">The azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueName">The name of the queue to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        string endpoint,
        string queueName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusQueueOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusQueue(
            _ => endpoint,
            _ => queueName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for specified Azure Service Bus Queue.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpointFactory">A factory to build the azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="queueNameFactory">A factory to build the name of the queue to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azurequeue' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusQueue(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> endpointFactory,
        Func<IServiceProvider, string> queueNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusQueueOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(endpointFactory);
        Guard.ThrowIfNull(queueNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZUREQUEUE_NAME,
            sp =>
            {
                var options = new AzureServiceBusQueueOptions(queueNameFactory(sp))
                {
                    Endpoint = endpointFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusQueueHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The topic name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        Action<AzureServiceBusTopicOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusTopic(
            _ => connectionString,
            _ => topicName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the topic name of the topic to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Action<AzureServiceBusTopicOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicOptions(topicNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpoint">The azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The topic name of the topic to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        string endpoint,
        string topicName,
        TokenCredential tokenCredential,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusTopic(
            _ => endpoint,
            _ => topicName,
            _ => tokenCredential,
            configure: null,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Topic.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpointFactory">A factory to build the azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the topic name of the topic to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusTopic(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> endpointFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusTopicOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(endpointFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURETOPIC_NAME,
            sp =>
            {
                var options = new AzureServiceBusTopicOptions(topicNameFactory(sp))
                {
                    Endpoint = endpointFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusTopicHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionString">The Azure ServiceBus connection string to be used.</param>
    /// <param name="topicName">The topic name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        string connectionString,
        string topicName,
        string subscriptionName,
        Action<AzureServiceBusSubscriptionOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusSubscription(
            _ => connectionString,
            _ => topicName,
            _ => subscriptionName,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="connectionStringFactory">A factory to build the Azure ServiceBus connection string to be used.</param>
    /// <param name="topicNameFactory">A factory to build the topic name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build The subscription name of the topic subscription to check.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> connectionStringFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Action<AzureServiceBusSubscriptionOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(connectionStringFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    ConnectionString = connectionStringFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpoint">The azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicName">The topic name of the topic to check.</param>
    /// <param name="subscriptionName">The subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredential">The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        string endpoint,
        string topicName,
        string subscriptionName,
        TokenCredential tokenCredential,
        Action<AzureServiceBusSubscriptionOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default) =>
        builder.AddAzureServiceBusSubscription(
            _ => endpoint,
            _ => topicName,
            _ => subscriptionName,
            _ => tokenCredential,
            configure,
            name,
            failureStatus,
            tags,
            timeout);

    /// <summary>
    /// Add a health check for Azure Service Bus Subscription.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="endpointFactory">A factory to build the azure service bus endpoint to be used, format sb://myservicebus.servicebus.windows.net/.</param>
    /// <param name="topicNameFactory">A factory to build the topic name of the topic to check.</param>
    /// <param name="subscriptionNameFactory">A factory to build The subscription name of the topic subscription to check.</param>
    /// <param name="tokenCredentialFactory">A factory to build The token credential for authentication.</param>
    /// <param name="configure">An optional action to allow additional Azure Service Bus configuration.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'azuretopic' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, string> endpointFactory,
        Func<IServiceProvider, string> topicNameFactory,
        Func<IServiceProvider, string> subscriptionNameFactory,
        Func<IServiceProvider, TokenCredential> tokenCredentialFactory,
        Action<AzureServiceBusSubscriptionOptions>? configure = default,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        Guard.ThrowIfNull(endpointFactory);
        Guard.ThrowIfNull(topicNameFactory);
        Guard.ThrowIfNull(subscriptionNameFactory);
        Guard.ThrowIfNull(tokenCredentialFactory);

        return builder.Add(new HealthCheckRegistration(
            name ?? AZURESUBSCRIPTION_NAME,
            sp =>
            {
                var options = new AzureServiceBusSubscriptionOptions(topicNameFactory(sp), subscriptionNameFactory(sp))
                {
                    Endpoint = endpointFactory(sp),
                    Credential = tokenCredentialFactory(sp)
                };

                configure?.Invoke(options);
                return new AzureServiceBusSubscriptionHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
