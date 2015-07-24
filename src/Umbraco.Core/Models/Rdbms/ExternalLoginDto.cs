using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoExternalLogin")]
    [ExplicitColumns]
    [PrimaryKey("Id")]
    internal class ExternalLoginDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoExternalLogin")]
        public int Id { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [Column("loginProvider")]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string LoginProvider { get; set; }

        [Column("providerKey")]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ProviderKey { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }


    }
}