using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserLogins")]
    [ExplicitColumns]
    internal class UserLoginDto
    {
        [Column("contextID")]
        public Guid ContextId { get; set; }

        [Column("userID")]
        public int UserId { get; set; }

        [Column("timeout")]
        public long Timeout { get; set; }
    }
}