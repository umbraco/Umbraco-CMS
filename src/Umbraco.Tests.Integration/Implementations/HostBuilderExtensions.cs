using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Umbraco.Core.Persistence;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration
{
    public static class HostBuilderExtensions
    {
        

        /// <summary>
        /// Ensures the lifetime of the host ends as soon as code executes
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseTestLifetime(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, collection) => collection.AddSingleton<IHostLifetime, TestLifetime>());
            return hostBuilder;
        }

        public static IHostBuilder UseLocalDb(this IHostBuilder hostBuilder, string dbFilePath)
        {
            // Need to register SqlClient manually
            // TODO: Move this to someplace central
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            hostBuilder.ConfigureAppConfiguration(x =>
            {
                if (!Directory.Exists(dbFilePath))
                    Directory.CreateDirectory(dbFilePath);

                var dbName = Guid.NewGuid().ToString("N");
                var instance = TestLocalDb.EnsureLocalDbInstanceAndDatabase(dbName, dbFilePath);

                x.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("ConnectionStrings:umbracoDbDSN", instance.GetConnectionString(dbName))
                });
            });
            return hostBuilder;
        }

        
    }


}
