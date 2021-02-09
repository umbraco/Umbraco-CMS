using System;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Extensions
{
    public static class ConfigConnectionStringExtensions
    {
        public static bool IsConnectionStringConfigured(this ConfigConnectionString databaseSettings)
        {
            var dbIsSqlCe = false;
            if (databaseSettings?.ProviderName != null)
            {
                dbIsSqlCe = databaseSettings.ProviderName == Constants.DbProviderNames.SqlCe;
            }

            var sqlCeDatabaseExists = false;
            if (dbIsSqlCe)
            {
                var parts = databaseSettings.ConnectionString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var dataSourcePart = parts.FirstOrDefault(x => x.InvariantStartsWith("Data Source="));
                if (dataSourcePart != null)
                {
                    var datasource = dataSourcePart.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
                    var filePath = datasource.Replace("Data Source=", string.Empty);
                    sqlCeDatabaseExists = File.Exists(filePath);
                }
            }

            // Either the connection details are not fully specified or it's a SQL CE database that doesn't exist yet
            if (databaseSettings == null
                || string.IsNullOrWhiteSpace(databaseSettings.ConnectionString) || string.IsNullOrWhiteSpace(databaseSettings.ProviderName)
                || (dbIsSqlCe && sqlCeDatabaseExists == false))
            {
                return false;
            }

            return true;
        }
    }
}
