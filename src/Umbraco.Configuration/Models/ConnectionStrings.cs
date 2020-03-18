using System;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Configuration.Models
{
    public class ConnectionStrings : IConnectionStrings
    {
        private readonly IConfiguration _configuration;

        public ConnectionStrings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ConfigConnectionString this[string key]
        {
            get => new ConfigConnectionString(_configuration.GetConnectionString(key), "System.Data.SqlClient", key);
            set => throw new NotImplementedException();
        }

        public void RemoveConnectionString(string umbracoConnectionName, IIOHelper ioHelper)
        {
            //TODO We need to figure out what to do here.. We cond have another config setting, that tells with file(s) to update? or should we assume appsettings.json
            throw new NotImplementedException();
        }

        public void SaveConnectionString(string connectionString, string providerName, IIOHelper ioHelper)
        {
            //TODO We need to figure out what to do here.. We cond have another config setting, that tells with file(s) to update? or should we assume appsettings.json
            throw new NotImplementedException();
        }
    }
}
