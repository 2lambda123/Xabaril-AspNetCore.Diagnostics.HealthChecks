﻿using HealthChecks.UI.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksUIBuilderExtensions
    {
        public static HealthChecksUIBuilder AddMySqlStorage(this HealthChecksUIBuilder builder, string connectionString, Action<DbContextOptionsBuilder> configureOptions = null)
        {
            builder.Services.AddDbContext<HealthChecksDb>(options =>
            {
                configureOptions?.Invoke(options);
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), s => s.MigrationsAssembly("HealthChecks.UI.MySql.Storage"));
            });

            return builder;
        }
    }
}
