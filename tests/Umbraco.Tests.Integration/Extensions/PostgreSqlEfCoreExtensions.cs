using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Extensions
{
    public static class PostgreSqlEfCoreExtensions
    {
        /// <summary>
        /// Sets the database provider. I.E UseSqlite or UseSqlServer based on the provider name.
        /// </summary>
        /// <param name="builder">The DbContext options builder.</param>
        /// <param name="providerName">The database provider name.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="InvalidDataException">Thrown when the provider is not supported.</exception>
        /// <remarks>
        /// Only supports the databases normally supported in Umbraco.
        /// </remarks>
        public static void UseCustomDatabaseProvider(this DbContextOptionsBuilder builder, string providerName, string connectionString)
        {
            switch (providerName)
            {
                case Core.Constants.ProviderNames.SQLServer:
                    builder.UseSqlServer(connectionString);
                    break;
                case Core.Constants.ProviderNames.SQLLite:
                case "Microsoft.Data.SQLite":
                    builder.UseSqlite(connectionString);
                    break;
                case "Npgsql":
                case "Npgsql2":
                    builder.UseNpgsql(connectionString);
                    break;
                default:
                    throw new InvalidDataException($"The provider {providerName} is not supported. Manually add the add the UseXXX statement to the options. I.E UseNpgsql()");
            }
        }

        /// <summary>
        /// Sets the database provider to use based on the Umbraco connection string.
        /// </summary>
        /// <param name="builder">The DbContext options builder.</param>
        /// <param name="serviceProvider">The service provider to resolve connection string settings from.</param>
        public static void UseCustomUmbracoDatabaseProvider(this DbContextOptionsBuilder builder, IServiceProvider serviceProvider)
        {
            ConnectionStrings connectionStrings = serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

            // Replace data directory
            string? dataDirectory = AppDomain.CurrentDomain.GetData(Core.Constants.System.DataDirectoryName)?.ToString();
            if (string.IsNullOrEmpty(dataDirectory) is false)
            {
                connectionStrings.ConnectionString = connectionStrings.ConnectionString?.Replace(Core.Constants.System.DataDirectoryPlaceholder, dataDirectory);
            }

            if (string.IsNullOrEmpty(connectionStrings.ProviderName))
            {
                Log.Warning("No database provider was set. ProviderName is null");
                return;
            }

            if (string.IsNullOrEmpty(connectionStrings.ConnectionString))
            {
                Log.Warning("No database provider was set. Connection string is null");
                return;
            }

            builder.UseCustomDatabaseProvider(connectionStrings.ProviderName, connectionStrings.ConnectionString);
        }
    }
}
