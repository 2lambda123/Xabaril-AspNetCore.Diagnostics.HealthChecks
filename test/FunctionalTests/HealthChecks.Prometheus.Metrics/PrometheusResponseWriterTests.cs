﻿using FluentAssertions;
using FunctionalTests.Base;
using FunctionalTests.HealthChecks.Publisher.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.Prometheus.Metrics
{
    [Collection("execution")]
    public class prometheus_responsewriter_should
    {
        private readonly ExecutionFixture _fixture;

        public prometheus_responsewriter_should(ExecutionFixture fixture)
        {
            _fixture = fixture;
        }

        [SkipOnAppVeyor]
        public async Task be_healthy_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Healthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecksPrometheusExporter("/health");
                }));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Healthy);
            resultAsString.Should().Contain("health_status 2");
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_and_return_503_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecksPrometheusExporter("/health");
                }));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Unhealthy);
            resultAsString.Should().Contain("health_status 0");
        }

        [SkipOnAppVeyor]
        public async Task be_unhealthy_and_return_configured_status_code_when_health_checks_are()
        {
            var sut = new TestServer(new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("fake", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app =>
                {
                    app.UseHealthChecksPrometheusExporter("/health", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK);
                }));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("fake", HealthStatus.Unhealthy);
            resultAsString.Should().Contain("health_status 0");
        }

        [SkipOnAppVeyor]
        public async Task be_health_status_unhealthy_when_one_health_check_is_healthy_and_another_unhealthy()
        {
            var sut = new TestServer(new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddCheck("healthy", check => HealthCheckResult.Healthy())
                        .AddCheck("unhealthy", check => HealthCheckResult.Unhealthy());
                })
                .Configure(app => app.UseHealthChecksPrometheusExporter("/health", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK)));

            var response = await sut.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
            var resultAsString = await response.Content.ReadAsStringAsync();
            resultAsString.Should().ContainCheckAndResult("healthy", HealthStatus.Healthy);
            resultAsString.Should().ContainCheckAndResult("unhealthy", HealthStatus.Unhealthy);
            resultAsString.Should().Contain("health_status 0");
        }
    }
}
