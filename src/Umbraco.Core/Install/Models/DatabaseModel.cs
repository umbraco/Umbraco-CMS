using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models
{
    [DataContract(Name = "database", Namespace = "")]
    public class DatabaseModel
    {
        [DataMember(Name = "dbType")]
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

        [DataMember(Name = "server")]
        public string Server { get; set; } = null!;

        [DataMember(Name = "databaseName")]
        public string DatabaseName { get; set; } = null!;

        [DataMember(Name = "login")]
        public string Login { get; set; } = null!;

        [DataMember(Name = "password")]
        public string Password { get; set; } = null!;

        [DataMember(Name = "integratedAuth")]
        public bool IntegratedAuth { get; set; }

        [DataMember(Name = "connectionString")]
        public string? ConnectionString { get; set; }
    }
}
