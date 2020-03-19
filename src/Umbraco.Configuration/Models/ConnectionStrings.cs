using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ConnectionStrings : IConnectionStrings
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
    }
}
