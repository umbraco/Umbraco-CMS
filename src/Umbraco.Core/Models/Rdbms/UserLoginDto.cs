using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserLogins")]
    [ExplicitColumns]
    internal class UserLoginDto
    {
        [Column("contextID")]
        [Index(IndexTypes.Clustered, Name = "IX_umbracoUserLogins_Index")]
        public Guid ContextId { get; set; }

        [Column("userID")]
        public int UserId { get; set; }

        [Column("timeout")]
        public long Timeout { get; set; }
    }
}