using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

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
        [Length(100)]
        public string Address { get; set; }

        [Column("registeredDate")]
        [Constraint(Default = "getdate()")]
        public DateTime DateRegistered { get; set; }

        [Column("lastNotifiedDate")]
        [Constraint(Default = "getdate()")]
        public DateTime LastNotified { get; set; }

        [Column("isActive")]
        [Index(IndexTypes.NonClustered)]
        public bool IsActive { get; set; }
    }
}