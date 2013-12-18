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
        [Length(500)]
        public string Address { get; set; }

        /// <summary>
        /// A unique column in the database, a computer name must always be unique!
        /// </summary>
        [Column("computerName")]
        [Length(255)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_computerName")]
        public string ComputerName { get; set; }

        [Column("registeredDate")]
        [Constraint(Default = "getdate()")]
        public DateTime DateRegistered { get; set; }

        [Column("lastNotifiedDate")]
        public DateTime LastNotified { get; set; }

        [Column("isActive")]
        [Index(IndexTypes.NonClustered)]
        public bool IsActive { get; set; }


    }
}