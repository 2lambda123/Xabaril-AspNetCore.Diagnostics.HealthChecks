using System.Net;

namespace HealthChecks.Npgsql.Tests.Functional
{
    public class DBConfigSetting
    {
        public string ConnectionString { get; set; } = null!;
        public string ConnectionString2 { get; set; } = null!;
    }

    public class npgsql_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_if_npgsql_is_available()
        {
            var connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddNpgSql(connectionString, tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
              .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_sql_query_is_not_valid()
        {
            var connectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres";

            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddNpgSql(connectionString, "SELECT 1 FROM InvalidDB", tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_unhealthy_if_npgsql_is_not_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                    .AddNpgSql("Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres", tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.StatusCode
                .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_npgsql_is_available_by_iServiceProvider_registered()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                    });

                    services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                                       .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_npgsql_is_not_available_registered()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = "Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                    });

                    services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                                       .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task be_healthy_if_multiple_npgsql_all_available()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres",
                        ConnectionString2 = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                    });

                    services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, name: "avalable1", tags: new string[] { "npgsql" })
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString2, name: "avalable2", tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                                       .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task be_unhealthy_if_multiple_npgsql_some_unavailable()
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(new DBConfigSetting
                    {
                        ConnectionString = "Server=200.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres",
                        ConnectionString2 = "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=postgres"
                    });

                    services.AddHealthChecks()
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString, name: "unavalable", tags: new string[] { "npgsql" })
                            .AddNpgSql(_ => _.GetRequiredService<DBConfigSetting>().ConnectionString2, name: "avalable", tags: new string[] { "npgsql" });
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = r => r.Tags.Contains("npgsql")
                    });
                });

            using var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest("/health")
                                       .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.ServiceUnavailable);
        }
    }
}
