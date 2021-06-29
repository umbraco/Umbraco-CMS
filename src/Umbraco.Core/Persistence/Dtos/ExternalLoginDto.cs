using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.ExternalLogin)]
    [ExplicitColumns]
    [PrimaryKey("Id")]
    internal class ExternalLoginDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoExternalLogin")]
        public int Id { get; set; }

        // TODO: This is completely missing a FK!!?

        [Column("userId")]
        public int UserId { get; set; }

        // TODO: There should be an index on both LoginProvider and ProviderKey

        [Column("loginProvider")]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string LoginProvider { get; set; }

        [Column("providerKey")]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ProviderKey { get; set; }

        [Column("createDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Used to store any arbitrary data for the user and external provider - like user tokens returned from the provider
        /// </summary>
        [Column("userData")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string UserData { get; set; }
    }
}
