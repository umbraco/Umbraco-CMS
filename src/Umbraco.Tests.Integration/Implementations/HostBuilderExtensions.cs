using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Umbraco.Tests.Integration.Testing;

namespace Umbraco.Tests.Integration.Implementations
{
    public static class HostBuilderExtensions
    {
        
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
