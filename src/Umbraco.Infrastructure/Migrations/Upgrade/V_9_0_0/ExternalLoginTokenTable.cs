using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

public class ExternalLoginTokenTable : MigrationBase
{
    public ExternalLoginTokenTable(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    ///     Adds new External Login token table
    /// </summary>
    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        if (tables.InvariantContains(ExternalLoginTokenDto.TableName))
        {
            return;
        }

        Create.Table<ExternalLoginTokenDto>().Do();
    }

    [TableName(TableName)]
    [ExplicitColumns]
    [PrimaryKey("Id")]
    internal class LegacyExternalLoginDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.ExternalLogin;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Obsolete(
            "This only exists to ensure you can upgrade using external logins from umbraco version where this was used to the new where it is not used")]
        [Column("userId")]
        public int? UserId { get; set; }

        /// <summary>
        ///     Used to store the name of the provider (i.e. Facebook, Google)
        /// </summary>
        [Column("loginProvider")]
        [Length(400)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, ForColumns = "loginProvider,userOrMemberKey",
            Name = "IX_" + TableName + "_LoginProvider")]
        public string LoginProvider { get; set; } = null!;

        /// <summary>
        ///     Stores the key the provider uses to lookup the login
        /// </summary>
        [Column("providerKey")]
        [Length(4000)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.NonClustered, ForColumns = "loginProvider,providerKey",
            Name = "IX_" + TableName + "_ProviderKey")]
        public string ProviderKey { get; set; } = null!;

        [Column("createDate")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        ///     Used to store any arbitrary data for the user and external provider - like user tokens returned from the provider
        /// </summary>
        [Column("userData")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        public string? UserData { get; set; }
    }
}
