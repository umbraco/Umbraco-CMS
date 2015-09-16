using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoServer")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ServerRegistrationDto
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("address")]
        [Length(500)]
        public string ServerAddress { get; set; }

        [Column("computerName")]
        [Length(255)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_computerName")] // server identity is unique
        public string ServerIdentity { get; set; }

        [Column("registeredDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime DateRegistered { get; set; }

        [Column("lastNotifiedDate")]
        public DateTime DateAccessed { get; set; }

        [Column("isActive")]
        [Index(IndexTypes.NonClustered)]
        public bool IsActive { get; set; }

        [Column("isMaster")]
        public bool IsMaster { get; set; }
    }
}