using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [Obsolete("Table no longer exists as of 7.7 - retained only to support migrations from previous versions")]
    [TableName("umbracoUserType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class UserTypeDto
    {
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 5)]
        public short Id { get; set; }

        [Column("userTypeAlias")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Alias { get; set; }

        [Column("userTypeName")]
        public string Name { get; set; }

        [Column("userTypeDefaultPermissions")]
        [Length(50)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string DefaultPermissions { get; set; }
    }
}