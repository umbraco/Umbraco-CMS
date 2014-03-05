using System.Runtime.Serialization;

namespace Umbraco.Web.Install.Models
{
    [DataContract(Name = "database", Namespace = "")]
    public class DatabaseModel
    {
        public DatabaseModel()
        {
            //defaults
            DatabaseType = DatabaseType.SqlCe;
        }

        [DataMember(Name = "dbType")]
        public DatabaseType DatabaseType { get; set; }

        [DataMember(Name = "server")]
        public string Server { get; set; }

        [DataMember(Name = "databaseName")]
        public string DatabaseName { get; set; }

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