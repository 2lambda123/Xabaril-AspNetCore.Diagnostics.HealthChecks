﻿using FluentAssertions;
using HealthChecks.AzureServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using UnitTests.DependencyInjection.AzureServiceBus;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureServiceBus
{
    public class azure_event_hub_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(connectionString, "hubName");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void add_health_check_without_event_hub_name_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            const string eventHubName = "samplehub";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName, eventHubName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(connectionString);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhub");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            const string namespaceName = "dummynamespace";
            var connectionString = AzureServiceBusConnectionStringGenerator.Generate(namespaceName);
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(connectionString, "hubName", name: "azureeventhubcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azureeventhubcheck");
            check.GetType().Should().Be(typeof(AzureEventHubHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureEventHub(string.Empty, string.Empty);

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
