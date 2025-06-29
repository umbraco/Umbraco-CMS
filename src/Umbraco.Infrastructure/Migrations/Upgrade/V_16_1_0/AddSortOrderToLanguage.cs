using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_1_0;

public class AddSortOrderToLanguage : UnscopedMigrationBase
{
    private const string NewColumnName = "sortOrder";
    private readonly IScopeProvider _scopeProvider;

    public AddSortOrderToLanguage(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
        => _scopeProvider = scopeProvider;

    protected override void Migrate()
    {
        // If the new column already exists we'll do nothing.
        if (ColumnExists(Constants.DatabaseSchema.Tables.Language, NewColumnName))
        {
            Context.Complete();
            return;
        }

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        // SQL server can simply add the column, but for SQLite this won't work,
        // so we'll have to create a new table and copy over data.
        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateSqlServer();
        }
        else
        {
            MigrateSqlite();
        }

        Context.Complete();
        scope.Complete();
    }

    private void MigrateSqlServer()
    {
        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<LanguageDto>(columns, NewColumnName);
    }

    private void MigrateSqlite()
    {
        /*
         * We commit the initial transaction started by the scope. This is required in order to disable the foreign keys.
         * We then begin a new transaction, this transaction will be committed or rolled back by the scope, like normal.
         * We don't have to worry about re-enabling the foreign keys, since these are enabled by default every time a connection is established.
         *
         * Ideally we'd want to do this with the unscoped database we get, however, this cannot be done,
         * since our scoped database cannot share a connection with the unscoped database, so a new one will be created, which enables the foreign keys.
         * Similarly we cannot use Database.CompleteTransaction(); since this also closes the connection,
         * so starting a new transaction would re-enable foreign keys.
         */
        Database.Execute("COMMIT;");
        Database.Execute("PRAGMA foreign_keys=off;");
        Database.Execute("BEGIN TRANSACTION;");

        IEnumerable<LanguageDto> languages = Database.Fetch<OldLanguageDto>().Select(x => new LanguageDto
        {
            Id = x.Id,
            IsoCode = x.IsoCode,
            CultureName = x.CultureName,
            IsDefault = x.IsDefault,
            IsMandatory = x.IsMandatory,
            FallbackLanguageId = x.FallbackLanguageId,
            SortOrder = 0
        });

        Delete.Table(Constants.DatabaseSchema.Tables.Language).Do();
        Create.Table<LanguageDto>().Do();

        foreach (LanguageDto language in languages)
        {
            Database.Insert(Constants.DatabaseSchema.Tables.Language, "id", false, language);
        }
    }

    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class OldLanguageDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.Language;

        // Public constants to bind properties between DTOs
        public const string IsoCodeColumnName = "languageISOCode";

        /// <summary>
        ///     Gets or sets the identifier of the language.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = 2)]
        public short Id { get; set; }

        /// <summary>
        ///     Gets or sets the ISO code of the language.
        /// </summary>
        [Column(IsoCodeColumnName)]
        [Index(IndexTypes.UniqueNonClustered)]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(14)]
        public string? IsoCode { get; set; }

        /// <summary>
        ///     Gets or sets the culture name of the language.
        /// </summary>
        [Column("languageCultureName")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(100)]
        public string? CultureName { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the language is the default language.
        /// </summary>
        [Column("isDefaultVariantLang")]
        [Constraint(Default = "0")]
        public bool IsDefault { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the language is mandatory.
        /// </summary>
        [Column("mandatory")]
        [Constraint(Default = "0")]
        public bool IsMandatory { get; set; }

        /// <summary>
        ///     Gets or sets the identifier of a fallback language.
        /// </summary>
        [Column("fallbackLanguageId")]
        [ForeignKey(typeof(LanguageDto), Column = "id")]
        [Index(IndexTypes.NonClustered)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? FallbackLanguageId { get; set; }
    }
}
