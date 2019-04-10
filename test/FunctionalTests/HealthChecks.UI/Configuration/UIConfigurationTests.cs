using System.IO;
using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace FunctionalTests.UI.Configuration
{
    public class UI_Configuration_should
    {
        [Fact]
        public void initialize_configuration_using_AddHealthChecksUI_setup_fluent_api()
        {
            var healthCheckName = "api1";
            var healthCheckUri = "http://api1/health";
            var webhookName = "webhook1";
            var webhookUri = "http://webhook1/sample";
            var webhookPayload = "payload1";
            var webhookRestorePayload = "restoredpayload1";
            var storageProvider = StorageProvider.Sqlite;
            var databaseConnection = "Data Source=healthchecksdb";
            var evaluationTimeInSeconds = 180;
            var minimumSeconds = 30;

            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: settings =>
                    {
                        settings
                            .AddHealthCheckEndpoint(name: healthCheckName, uri: healthCheckUri)
                            .AddWebhookNotification(name: webhookName, uri: webhookUri, payload: webhookPayload,
                                restorePayload: webhookRestorePayload)
                            .SetEvaluationTimeInSeconds(evaluationTimeInSeconds)
                            .SetMinimumSecondsBetweenFailureNotifications(minimumSeconds)
                            .SetHealthCheckData(storageProvider, databaseConnection);
                    });
                });
            
            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;

            UISettings.EvaluationTimeInSeconds.Should().Be(evaluationTimeInSeconds);
            UISettings.Data.Provider.Should().Be(storageProvider);
            UISettings.Data.ConnectionString.Should().Be(databaseConnection);
            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(minimumSeconds);

            UISettings.Webhooks.Count.Should().Be(1);
            UISettings.HealthChecks.Count.Should().Be(1);

            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.Should().Be(healthCheckName);
            healthcheck.Uri.Should().Be(healthCheckUri);

            var webhook = UISettings.Webhooks[0];
            webhook.Name.Should().Be(webhookName);
            webhook.Uri.Should().Be(webhookUri);
            webhook.Payload.Should().Be(webhookPayload);
            webhook.RestoredPayload.Should().Be(webhookRestorePayload);

        }

        [Fact]
        public void load_ui_settings_from_configuration_key()
        {
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
                    
                }).ConfigureServices(services => { services.AddHealthChecksUI(); });


            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;


            UISettings.EvaluationTimeInSeconds.Should().Be(20);
            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(120);
            UISettings.Data.Provider.Should().Be(StorageProvider.Sqlite);
            UISettings.Data.ConnectionString.Should().Be("Data Source=healthchecksdb");

            UISettings.HealthChecks.Count.Should().Be(1);
            UISettings.Webhooks.Count.Should().Be(1);
            
            var healthcheck = UISettings.HealthChecks[0];
            healthcheck.Name.Should().Be("api1");
            healthcheck.Uri.Should().Be("http://api1/healthz");
            
            
            var webhook = UISettings.Webhooks[0];
            webhook.Name.Should().Be("webhook1");
            webhook.Uri.Should().Be("http://webhook1");
            webhook.Payload.Should().Be("payload");
            webhook.RestoredPayload.Should().Be("restoredpayload");
        }

        [Fact]
        public void support_combined_configuration_from_fluent_api_and_settings_key()
        {
            var healthCheckName = "api2";
            var healthCheckUri = "http://api2/healthz";
            var webhookName = "webhook2";
            var webhookUri = "http://webhook2";
            var webhookPayload = "payload1";
            
            var webhost = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureAppConfiguration(conf =>
                {
                    conf.Sources.Clear();
                    conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
                    
                }).ConfigureServices(services =>
                {
                    services.AddHealthChecksUI(setupSettings: setup =>
                    {
                        setup
                            .AddHealthCheckEndpoint(name: healthCheckName, uri: healthCheckUri)
                            .AddWebhookNotification(name: webhookName, uri: webhookUri, payload: webhookPayload)
                            .SetMinimumSecondsBetweenFailureNotifications(200);
                    });
                });

            var serviceProvider = webhost.Build().Services;
            var UISettings = serviceProvider.GetService<IOptions<Settings>>().Value;

            UISettings.MinimumSecondsBetweenFailureNotifications.Should().Be(200);
            UISettings.EvaluationTimeInSeconds.Should().Be(20);
            UISettings.Data.Provider.Should().Be(StorageProvider.Sqlite);
            UISettings.Data.ConnectionString.Should().Be("Data Source=healthchecksdb");
            UISettings.Webhooks.Count.Should().Be(2);
            UISettings.HealthChecks.Count.Should().Be((2));

            var healthCheck1 = UISettings.HealthChecks[0];
            var healthCheck2 = UISettings.HealthChecks[1];
            var webHook1 = UISettings.Webhooks[0];
            var webHook2 = UISettings.Webhooks[1];

            healthCheck1.Name.Should().Be("api1");
            healthCheck1.Uri.Should().Be("http://api1/healthz");
            healthCheck2.Name.Should().Be("api2");
            healthCheck2.Uri.Should().Be("http://api2/healthz");

            webHook1.Name.Should().Be("webhook1");
            webHook1.Uri.Should().Be("http://webhook1");
            webHook2.Name.Should().Be(webhookName);
            webHook2.Uri.Should().Be(webhookUri);

        }
    }
}