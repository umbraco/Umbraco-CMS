using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserLogin")]
    [PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    internal class UserLoginsDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_userLogins")]
        public int Id { get; set; }

        [Column("userId")]
        [ForeignKey(typeof(UserDto), Name = "FK_umbracoUserLogin_umbracoUser_id")]
        public int UserId { get; set; }

        [Column("createdUtc")]
        public DateTime CreatedUtc { get; set; }
        
        [Column("sessionId")]
        public string SessionId { get; set; }
    }
}