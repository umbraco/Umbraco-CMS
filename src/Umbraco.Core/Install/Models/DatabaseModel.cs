using System;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models
{
    [DataContract(Name = "database", Namespace = "")]
    public class DatabaseModel
    {
        public const string DefaultDatabaseName = "umbraco-cms";

        [DataMember(Name = "databaseProviderMetadataId")]
        public Guid DatabaseProviderMetadataId { get; set; }

        [DataMember(Name = "providerName")]
        public string ProviderName { get; set; }

        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "databaseName")]
        public string DatabaseName { get; set; } = DefaultDatabaseName;

        [DataMember(Name = "login")]
        public string Login { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "integratedAuth")]
        public bool IntegratedAuth { get; set; }

        [DataMember(Name = "connectionString")]
        public string ConnectionString { get; set; }
    }
}
