using System.Collections.Generic;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoUserGroup")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UserGroupDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 5)]
        public int Id { get; set; }

        [Column("userGroupAlias")]
        [Length(200)]
        public string Alias { get; set; }

        [Column("userGroupName")]
        [Length(200)]
        public string Name { get; set; }

        [Column("userGroupDefaultPermissions")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DefaultPermissions { get; set; }

        [ResultColumn]
        public List<UserGroup2AppDto> UserGroup2AppDtos { get; set; }
    }
}