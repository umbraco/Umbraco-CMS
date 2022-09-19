using System.Data.SqlTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Infrastructure.Migrations.Install;

/// <summary>
///     Creates the initial database schema during install.
/// </summary>
public class DatabaseSchemaCreator
{
    // all tables, in order
    internal static readonly List<Type> _orderedTables = new()
    {
        typeof(UserDto),
        typeof(NodeDto),
        typeof(ContentTypeDto),
        typeof(TemplateDto),
        typeof(ContentDto),
        typeof(ContentVersionDto),
        typeof(MediaVersionDto),
        typeof(DocumentDto),
        typeof(ContentTypeTemplateDto),
        typeof(DataTypeDto),
        typeof(DictionaryDto),
        typeof(LanguageDto),
        typeof(LanguageTextDto),
        typeof(DomainDto),
        typeof(LogDto),
        typeof(MacroDto),
        typeof(MacroPropertyDto),
        typeof(MemberPropertyTypeDto),
        typeof(MemberDto),
        typeof(Member2MemberGroupDto),
        typeof(PropertyTypeGroupDto),
        typeof(PropertyTypeDto),
        typeof(PropertyDataDto),
        typeof(RelationTypeDto),
        typeof(RelationDto),
        typeof(TagDto),
        typeof(TagRelationshipDto),
        typeof(ContentType2ContentTypeDto),
        typeof(ContentTypeAllowedContentTypeDto),
        typeof(User2NodeNotifyDto),
        typeof(ServerRegistrationDto),
        typeof(AccessDto),
        typeof(AccessRuleDto),
        typeof(CacheInstructionDto),
        typeof(ExternalLoginDto),
        typeof(ExternalLoginTokenDto),
        typeof(TwoFactorLoginDto),
        typeof(RedirectUrlDto),
        typeof(LockDto),
        typeof(UserGroupDto),
        typeof(User2UserGroupDto),
        typeof(UserGroup2NodePermissionDto),
        typeof(UserGroup2AppDto),
        typeof(UserStartNodeDto),
        typeof(ContentNuDto),
        typeof(DocumentVersionDto),
        typeof(KeyValueDto),
        typeof(UserLoginDto),
        typeof(ConsentDto),
        typeof(AuditEntryDto),
        typeof(ContentVersionCultureVariationDto),
        typeof(DocumentCultureVariationDto),
        typeof(ContentScheduleDto),
        typeof(LogViewerQueryDto),
        typeof(ContentVersionCleanupPolicyDto),
        typeof(UserGroup2NodeDto),
        typeof(CreatedPackageSchemaDto),
        typeof(UserGroup2LanguageDto)
    };

    private readonly IUmbracoDatabase _database;
    private readonly IOptionsMonitor<InstallDefaultDataSettings> _defaultDataCreationSettings;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger<DatabaseSchemaCreator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IUmbracoVersion _umbracoVersion;

    [Obsolete("Please use constructor taking all parameters. Scheduled for removal in V11.")]
    public DatabaseSchemaCreator(
        IUmbracoDatabase? database,
        ILogger<DatabaseSchemaCreator> logger,
        ILoggerFactory loggerFactory,
        IUmbracoVersion umbracoVersion,
        IEventAggregator eventAggregator)
        : this(database, logger, loggerFactory, umbracoVersion, eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<InstallDefaultDataSettings>>())
    {
    }

    public DatabaseSchemaCreator(
        IUmbracoDatabase? database,
        ILogger<DatabaseSchemaCreator> logger,
        ILoggerFactory loggerFactory,
        IUmbracoVersion umbracoVersion,
        IEventAggregator eventAggregator,
        IOptionsMonitor<InstallDefaultDataSettings> defaultDataCreationSettings)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
        _eventAggregator = eventAggregator;
        _defaultDataCreationSettings = defaultDataCreationSettings;

        if (_database?.SqlContext?.SqlSyntax == null)
        {
            throw new InvalidOperationException("No SqlContext has been assigned to the database");
        }
    }

    private ISqlSyntaxProvider SqlSyntax => _database.SqlContext.SqlSyntax;

    /// <summary>
    ///     Drops all Umbraco tables in the db.
    /// </summary>
    internal void UninstallDatabaseSchema()
    {
        _logger.LogInformation("Start UninstallDatabaseSchema");

        foreach (Type table in _orderedTables.AsEnumerable().Reverse())
        {
            TableNameAttribute? tableNameAttribute = table.FirstAttribute<TableNameAttribute>();
            var tableName = tableNameAttribute == null ? table.Name : tableNameAttribute.Value;

            _logger.LogInformation("Uninstall {TableName}", tableName);

            try
            {
                if (TableExists(tableName))
                {
                    DropTable(tableName);
                }
            }
            catch (Exception ex)
            {
                //swallow this for now, not sure how best to handle this with diff databases... though this is internal
                // and only used for unit tests. If this fails its because the table doesn't exist... generally!
                _logger.LogError(ex, "Could not drop table {TableName}", tableName);
            }
        }
    }

    /// <summary>
    ///     Initializes the database by creating the umbraco db schema.
    /// </summary>
    /// <remarks>This needs to execute as part of a transaction.</remarks>
    public void InitializeDatabaseSchema()
    {
        if (!_database.InTransaction)
        {
            throw new InvalidOperationException("Database is not in a transaction.");
        }

        var eventMessages = new EventMessages();
        var creatingNotification = new DatabaseSchemaCreatingNotification(eventMessages);
        FireBeforeCreation(creatingNotification);

        if (creatingNotification.Cancel == false)
        {
            var dataCreation = new DatabaseDataCreator(
                _database, _loggerFactory.CreateLogger<DatabaseDataCreator>(),
                _umbracoVersion,
                _defaultDataCreationSettings);
            foreach (Type table in _orderedTables)
            {
                CreateTable(false, table, dataCreation);
            }
        }

        DatabaseSchemaCreatedNotification createdNotification =
            new DatabaseSchemaCreatedNotification(eventMessages).WithStateFrom(creatingNotification);
        FireAfterCreation(createdNotification);
    }

    /// <summary>
    ///     Validates the schema of the current database.
    /// </summary>
    internal DatabaseSchemaResult ValidateSchema() => ValidateSchema(_orderedTables);

    internal DatabaseSchemaResult ValidateSchema(IEnumerable<Type> orderedTables)
    {
        var result = new DatabaseSchemaResult();

        result.IndexDefinitions.AddRange(SqlSyntax.GetDefinedIndexes(_database)
            .Select(x => new DbIndexDefinition(x)));

        result.TableDefinitions.AddRange(orderedTables
            .Select(x => DefinitionFactory.GetTableDefinition(x, SqlSyntax)));

        ValidateDbTables(result);
        ValidateDbColumns(result);
        ValidateDbIndexes(result);
        ValidateDbConstraints(result);

        return result;
    }

    /// <summary>
    ///     This validates the Primary/Foreign keys in the database
    /// </summary>
    /// <param name="result"></param>
    /// <remarks>
    ///     This does not validate any database constraints that are not PKs or FKs because Umbraco does not create a database
    ///     with non PK/FK constraints.
    ///     Any unique "constraints" in the database are done with unique indexes.
    /// </remarks>
    private void ValidateDbConstraints(DatabaseSchemaResult result)
    {
        //Check constraints in configured database against constraints in schema
        var constraintsInDatabase = SqlSyntax.GetConstraintsPerColumn(_database).DistinctBy(x => x.Item3).ToList();
        var foreignKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("FK_"))
            .Select(x => x.Item3).ToList();
        var primaryKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("PK_"))
            .Select(x => x.Item3).ToList();

        var unknownConstraintsInDatabase = constraintsInDatabase.Where(
            x => x.Item3.InvariantStartsWith("FK_") == false && x.Item3.InvariantStartsWith("PK_") == false &&
                 x.Item3.InvariantStartsWith("IX_") == false
        ).Select(x => x.Item3).ToList();

        var foreignKeysInSchema = result.TableDefinitions.SelectMany(x => x.ForeignKeys.Select(y => y.Name))
            .Where(x => x is not null).ToList();
        var primaryKeysInSchema = result.TableDefinitions.SelectMany(x => x.Columns.Select(y => y.PrimaryKeyName))
            .Where(x => x.IsNullOrWhiteSpace() == false).ToList();

        // Add valid and invalid foreign key differences to the result object
        // We'll need to do invariant contains with case insensitivity because foreign key, primary key is not standardized
        // In theory you could have: FK_ or fk_ ...or really any standard that your development department (or developer) chooses to use.
        foreach (var unknown in unknownConstraintsInDatabase)
        {
            if (foreignKeysInSchema!.InvariantContains(unknown) || primaryKeysInSchema!.InvariantContains(unknown))
            {
                result.ValidConstraints.Add(unknown);
            }
            else
            {
                result.Errors.Add(new Tuple<string, string>("Unknown", unknown));
            }
        }

        // Foreign keys:
        IEnumerable<string?> validForeignKeyDifferences =
            foreignKeysInDatabase.Intersect(foreignKeysInSchema, StringComparer.InvariantCultureIgnoreCase);
        foreach (var foreignKey in validForeignKeyDifferences)
        {
            if (foreignKey is not null)
            {
                result.ValidConstraints.Add(foreignKey);
            }
        }

        IEnumerable<string?> invalidForeignKeyDifferences = foreignKeysInDatabase
            .Except(foreignKeysInSchema, StringComparer.InvariantCultureIgnoreCase)
            .Union(foreignKeysInSchema.Except(foreignKeysInDatabase, StringComparer.InvariantCultureIgnoreCase));
        foreach (var foreignKey in invalidForeignKeyDifferences)
        {
            result.Errors.Add(new Tuple<string, string>("Constraint", foreignKey ?? "NULL"));
        }

        // Primary keys:
        // Add valid and invalid primary key differences to the result object
        IEnumerable<string> validPrimaryKeyDifferences =
            primaryKeysInDatabase!.Intersect(primaryKeysInSchema, StringComparer.InvariantCultureIgnoreCase)!;
        foreach (var primaryKey in validPrimaryKeyDifferences)
        {
            result.ValidConstraints.Add(primaryKey);
        }

        IEnumerable<string> invalidPrimaryKeyDifferences =
            primaryKeysInDatabase!.Except(primaryKeysInSchema, StringComparer.InvariantCultureIgnoreCase)!
                .Union(primaryKeysInSchema.Except(primaryKeysInDatabase, StringComparer.InvariantCultureIgnoreCase))!;
        foreach (var primaryKey in invalidPrimaryKeyDifferences)
        {
            result.Errors.Add(new Tuple<string, string>("Constraint", primaryKey));
        }
    }

    private void ValidateDbColumns(DatabaseSchemaResult result)
    {
        //Check columns in configured database against columns in schema
        IEnumerable<ColumnInfo> columnsInDatabase = SqlSyntax.GetColumnsInSchema(_database);
        var columnsPerTableInDatabase =
            columnsInDatabase.Select(x => string.Concat(x.TableName, ",", x.ColumnName)).ToList();
        var columnsPerTableInSchema = result.TableDefinitions
            .SelectMany(x => x.Columns.Select(y => string.Concat(y.TableName, ",", y.Name))).ToList();
        //Add valid and invalid column differences to the result object
        IEnumerable<string> validColumnDifferences =
            columnsPerTableInDatabase.Intersect(columnsPerTableInSchema, StringComparer.InvariantCultureIgnoreCase);
        foreach (var column in validColumnDifferences)
        {
            result.ValidColumns.Add(column);
        }

        IEnumerable<string> invalidColumnDifferences =
            columnsPerTableInDatabase.Except(columnsPerTableInSchema, StringComparer.InvariantCultureIgnoreCase)
                .Union(columnsPerTableInSchema.Except(columnsPerTableInDatabase,
                    StringComparer.InvariantCultureIgnoreCase));
        foreach (var column in invalidColumnDifferences)
        {
            result.Errors.Add(new Tuple<string, string>("Column", column));
        }
    }

    private void ValidateDbTables(DatabaseSchemaResult result)
    {
        //Check tables in configured database against tables in schema
        var tablesInDatabase = SqlSyntax.GetTablesInSchema(_database).ToList();
        var tablesInSchema = result.TableDefinitions.Select(x => x.Name).ToList();
        //Add valid and invalid table differences to the result object
        IEnumerable<string?> validTableDifferences =
            tablesInDatabase.Intersect(tablesInSchema, StringComparer.InvariantCultureIgnoreCase);
        foreach (var tableName in validTableDifferences)
        {
            if (tableName is not null)
            {
                result.ValidTables.Add(tableName);
            }
        }

        IEnumerable<string?> invalidTableDifferences =
            tablesInDatabase.Except(tablesInSchema, StringComparer.InvariantCultureIgnoreCase)
                .Union(tablesInSchema.Except(tablesInDatabase, StringComparer.InvariantCultureIgnoreCase));
        foreach (var tableName in invalidTableDifferences)
        {
            result.Errors.Add(new Tuple<string, string>("Table", tableName ?? "NULL"));
        }
    }

    private void ValidateDbIndexes(DatabaseSchemaResult result)
    {
        //These are just column indexes NOT constraints or Keys
        //var colIndexesInDatabase = result.DbIndexDefinitions.Where(x => x.IndexName.InvariantStartsWith("IX_")).Select(x => x.IndexName).ToList();
        var colIndexesInDatabase = result.IndexDefinitions.Select(x => x.IndexName).ToList();
        var indexesInSchema = result.TableDefinitions.SelectMany(x => x.Indexes.Select(y => y.Name)).ToList();

        //Add valid and invalid index differences to the result object
        IEnumerable<string?> validColIndexDifferences =
            colIndexesInDatabase.Intersect(indexesInSchema, StringComparer.InvariantCultureIgnoreCase);
        foreach (var index in validColIndexDifferences)
        {
            if (index is not null)
            {
                result.ValidIndexes.Add(index);
            }
        }

        IEnumerable<string?> invalidColIndexDifferences =
            colIndexesInDatabase.Except(indexesInSchema, StringComparer.InvariantCultureIgnoreCase)
                .Union(indexesInSchema.Except(colIndexesInDatabase, StringComparer.InvariantCultureIgnoreCase));
        foreach (var index in invalidColIndexDifferences)
        {
            result.Errors.Add(new Tuple<string, string>("Index", index ?? "NULL"));
        }
    }

    #region Notifications

    /// <summary>
    ///     Publishes the <see cref="Notifications.DatabaseSchemaCreatingNotification" /> notification.
    /// </summary>
    /// <param name="notification">Cancelable notification marking the creation having begun.</param>
    internal virtual void FireBeforeCreation(DatabaseSchemaCreatingNotification notification) =>
        _eventAggregator.Publish(notification);

    /// <summary>
    ///     Publishes the <see cref="DatabaseSchemaCreatedNotification" /> notification.
    /// </summary>
    /// <param name="notification">Notification marking the creation having completed.</param>
    internal virtual void FireAfterCreation(DatabaseSchemaCreatedNotification notification) =>
        _eventAggregator.Publish(notification);

    #endregion

    #region Utilities

    /// <summary>
    ///     Returns whether a table with the specified <paramref name="tableName" /> exists in the database.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns><c>true</c> if the table exists; otherwise <c>false</c>.</returns>
    /// <example>
    ///     <code>
    /// if (schemaHelper.TableExist("MyTable"))
    /// {
    ///     // do something when the table exists
    /// }
    /// </code>
    /// </example>
    public bool TableExists(string? tableName) =>
        tableName is not null && SqlSyntax.DoesTableExist(_database, tableName);

    /// <summary>
    ///     Returns whether the table for the specified <typeparamref name="T" /> exists in the database.
    /// </summary>
    /// <typeparam name="T">The type representing the DTO/table.</typeparam>
    /// <returns><c>true</c> if the table exists; otherwise <c>false</c>.</returns>
    /// <example>
    ///     <code>
    /// if (schemaHelper.TableExist&lt;MyDto&gt;)
    /// {
    ///     // do something when the table exists
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    ///     If <typeparamref name="T" /> has been decorated with an <see cref="TableNameAttribute" />, the name from that
    ///     attribute will be used for the table name. If the attribute is not present, the name
    ///     <typeparamref name="T" /> will be used instead.
    /// </remarks>
    public bool TableExists<T>()
    {
        TableDefinition table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
        return table != null && TableExists(table.Name);
    }

    /// <summary>
    ///     Creates a new table in the database based on the type of <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type representing the DTO/table.</typeparam>
    /// <param name="overwrite">Whether the table should be overwritten if it already exists.</param>
    /// <remarks>
    ///     If <typeparamref name="T" /> has been decorated with an <see cref="TableNameAttribute" />, the name from that
    ///     attribute will be used for the table name. If the attribute is not present, the name
    ///     <typeparamref name="T" /> will be used instead.
    ///     If a table with the same name already exists, the <paramref name="overwrite" /> parameter will determine
    ///     whether the table is overwritten. If <c>true</c>, the table will be overwritten, whereas this method will
    ///     not do anything if the parameter is <c>false</c>.
    /// </remarks>
    internal void CreateTable<T>(bool overwrite = false)
        where T : new()
    {
        Type tableType = typeof(T);
        CreateTable(
            overwrite,
            tableType,
            new DatabaseDataCreator(
                _database,
                _loggerFactory.CreateLogger<DatabaseDataCreator>(),
                _umbracoVersion,
                _defaultDataCreationSettings));
    }

    /// <summary>
    ///     Creates a new table in the database for the specified <paramref name="modelType" />.
    /// </summary>
    /// <param name="overwrite">Whether the table should be overwritten if it already exists.</param>
    /// <param name="modelType">The representing the table.</param>
    /// <param name="dataCreation"></param>
    /// <remarks>
    ///     If <paramref name="modelType" /> has been decorated with an <see cref="TableNameAttribute" />, the name from
    ///     that  attribute will be used for the table name. If the attribute is not present, the name
    ///     <paramref name="modelType" /> will be used instead.
    ///     If a table with the same name already exists, the <paramref name="overwrite" /> parameter will determine
    ///     whether the table is overwritten. If <c>true</c>, the table will be overwritten, whereas this method will
    ///     not do anything if the parameter is <c>false</c>.
    ///     This need to execute as part of a transaction.
    /// </remarks>
    internal void CreateTable(bool overwrite, Type modelType, DatabaseDataCreator dataCreation)
    {
        if (!_database.InTransaction)
        {
            throw new InvalidOperationException("Database is not in a transaction.");
        }

        TableDefinition tableDefinition = DefinitionFactory.GetTableDefinition(modelType, SqlSyntax);
        var tableName = tableDefinition.Name;
        var tableExist = TableExists(tableName);
        if (string.IsNullOrEmpty(tableName))
        {
            throw new SqlNullValueException("Tablename was null");
        }

        if (overwrite && tableExist)
        {
            _logger.LogInformation("Table {TableName} already exists, but will be recreated", tableName);

            DropTable(tableName);
            tableExist = false;
        }

        if (tableExist)
        {
            // The table exists and was not recreated/overwritten.
            _logger.LogInformation("Table {TableName} already exists - no changes were made", tableName);
            return;
        }

        //Execute the Create Table sql
        SqlSyntax.HandleCreateTable(_database, tableDefinition);

        if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
        {
            // This should probably delegate to whole thing to the syntax provider
            _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} ON "));
        }

        //Call the NewTable-event to trigger the insert of base/default data
        //OnNewTable(tableName, _db, e, _logger);

        dataCreation.InitializeBaseData(tableName);

        if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
        {
            _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} OFF;"));
        }

        if (overwrite)
        {
            _logger.LogInformation("Table {TableName} was recreated", tableName);
        }
        else
        {
            _logger.LogInformation("New table {TableName} was created", tableName);
        }
    }

    /// <summary>
    ///     Drops the table for the specified <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type representing the DTO/table.</typeparam>
    /// <example>
    ///     <code>
    /// schemaHelper.DropTable&lt;MyDto&gt;);
    /// </code>
    /// </example>
    /// <remarks>
    ///     If <typeparamref name="T" /> has been decorated with an <see cref="TableNameAttribute" />, the name from that
    ///     attribute will be used for the table name. If the attribute is not present, the name
    ///     <typeparamref name="T" /> will be used instead.
    /// </remarks>
    public void DropTable(string? tableName)
    {
        var sql = new Sql(string.Format(SqlSyntax.DropTable, SqlSyntax.GetQuotedTableName(tableName)));
        _database.Execute(sql);
    }

    #endregion
}
