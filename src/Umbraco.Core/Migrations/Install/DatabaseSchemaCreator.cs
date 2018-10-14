﻿using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Install
{
    /// <summary>
    /// Creates the initial database schema during install.
    /// </summary>
    internal class DatabaseSchemaCreator
    {
        private readonly IUmbracoDatabase _database;
        private readonly ILogger _logger;

        public DatabaseSchemaCreator(IUmbracoDatabase database, ILogger logger)
        {
            _database = database;
            _logger = logger;
        }

        private ISqlSyntaxProvider SqlSyntax => _database.SqlContext.SqlSyntax;

        // all tables, in order
        public static readonly List<Type> OrderedTables = new List<Type>
        {
            typeof (UserDto),
            typeof (NodeDto),
            typeof (ContentTypeDto),
            typeof (TemplateDto),
            typeof (ContentDto),
            typeof (ContentVersionDto),
            typeof (MediaVersionDto),
            typeof (DocumentDto),
            typeof (ContentTypeTemplateDto),
            typeof (DataTypeDto),
            typeof (DictionaryDto),
            typeof (LanguageDto),
            typeof (LanguageTextDto),
            typeof (DomainDto),
            typeof (LogDto),
            typeof (MacroDto),
            typeof (MacroPropertyDto),
            typeof (MemberTypeDto),
            typeof (MemberDto),
            typeof (Member2MemberGroupDto),
            typeof (ContentXmlDto),
            typeof (PreviewXmlDto),
            typeof (PropertyTypeGroupDto),
            typeof (PropertyTypeDto),
            typeof (PropertyDataDto),
            typeof (RelationTypeDto),
            typeof (RelationDto),
            typeof (TagDto),
            typeof (TagRelationshipDto),
            typeof (ContentType2ContentTypeDto),
            typeof (ContentTypeAllowedContentTypeDto),
            typeof (User2NodeNotifyDto),
            typeof (ServerRegistrationDto),
            typeof (AccessDto),
            typeof (AccessRuleDto),
            typeof (CacheInstructionDto),
            typeof (ExternalLoginDto),
            typeof (RedirectUrlDto),
            typeof (LockDto),
            typeof (UserGroupDto),
            typeof (User2UserGroupDto),
            typeof (UserGroup2NodePermissionDto),
            typeof (UserGroup2AppDto),
            typeof (UserStartNodeDto),
            typeof (ContentNuDto),
            typeof (DocumentVersionDto),
            typeof (KeyValueDto),
            typeof (UserLoginDto),
            typeof (ConsentDto),
            typeof (AuditEntryDto),
            typeof (ContentVersionCultureVariationDto),
            typeof (DocumentCultureVariationDto)
        };

        /// <summary>
        /// Drops all Umbraco tables in the db.
        /// </summary>
        internal void UninstallDatabaseSchema()
        {
            _logger.Info<DatabaseSchemaCreator>("Start UninstallDatabaseSchema");

            foreach (var table in OrderedTables.AsEnumerable().Reverse())
            {
                var tableNameAttribute = table.FirstAttribute<TableNameAttribute>();
                var tableName = tableNameAttribute == null ? table.Name : tableNameAttribute.Value;

                _logger.Info<DatabaseSchemaCreator>("Uninstall {TableName}", tableName);

                try
                {
                    if (TableExists(tableName))
                        DropTable(tableName);
                }
                catch (Exception ex)
                {
                    //swallow this for now, not sure how best to handle this with diff databases... though this is internal
                    // and only used for unit tests. If this fails its because the table doesn't exist... generally!
                    _logger.Error<DatabaseSchemaCreator>(ex, "Could not drop table {TableName}", tableName);
                }
            }
        }

        /// <summary>
        /// Initializes the database by creating the umbraco db schema.
        /// </summary>
        public void InitializeDatabaseSchema()
        {
            var e = new DatabaseCreationEventArgs();
            FireBeforeCreation(e);

            if (e.Cancel == false)
            {
                var dataCreation = new DatabaseDataCreator(_database, _logger);
                foreach (var table in OrderedTables)
                    CreateTable(false, table, dataCreation);
            }

            FireAfterCreation(e);
        }

        /// <summary>
        /// Validates the schema of the current database.
        /// </summary>
        public DatabaseSchemaResult ValidateSchema()
        {
            var result = new DatabaseSchemaResult(SqlSyntax);

            //get the db index defs
            result.DbIndexDefinitions = SqlSyntax.GetDefinedIndexes(_database)
                .Select(x => new DbIndexDefinition(x)).ToArray();

            result.TableDefinitions.AddRange(OrderedTables
                .Select(x => DefinitionFactory.GetTableDefinition(x, SqlSyntax)));

            ValidateDbTables(result);
            ValidateDbColumns(result);
            ValidateDbIndexes(result);
            ValidateDbConstraints(result);

            return result;
        }

        /// <summary>
        /// This validates the Primary/Foreign keys in the database
        /// </summary>
        /// <param name="result"></param>
        /// <remarks>
        /// This does not validate any database constraints that are not PKs or FKs because Umbraco does not create a database with non PK/FK contraints.
        /// Any unique "constraints" in the database are done with unique indexes.
        /// </remarks>
        private void ValidateDbConstraints(DatabaseSchemaResult result)
        {
            //MySql doesn't conform to the "normal" naming of constraints, so there is currently no point in doing these checks.
            //TODO: At a later point we do other checks for MySql, but ideally it should be necessary to do special checks for different providers.
            // ALso note that to get the constraints for MySql we have to open a connection which we currently have not.
            if (SqlSyntax is MySqlSyntaxProvider)
                return;

            //Check constraints in configured database against constraints in schema
            var constraintsInDatabase = SqlSyntax.GetConstraintsPerColumn(_database).DistinctBy(x => x.Item3).ToList();
            var foreignKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("FK_")).Select(x => x.Item3).ToList();
            var primaryKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("PK_")).Select(x => x.Item3).ToList();

            var unknownConstraintsInDatabase =
                constraintsInDatabase.Where(
                    x =>
                    x.Item3.InvariantStartsWith("FK_") == false && x.Item3.InvariantStartsWith("PK_") == false &&
                    x.Item3.InvariantStartsWith("IX_") == false).Select(x => x.Item3).ToList();
            var foreignKeysInSchema = result.TableDefinitions.SelectMany(x => x.ForeignKeys.Select(y => y.Name)).ToList();
            var primaryKeysInSchema = result.TableDefinitions.SelectMany(x => x.Columns.Select(y => y.PrimaryKeyName))
                .Where(x => x.IsNullOrWhiteSpace() == false).ToList();

            //Add valid and invalid foreign key differences to the result object
            // We'll need to do invariant contains with case insensitivity because foreign key, primary key, and even index naming w/ MySQL is not standardized
            // In theory you could have: FK_ or fk_ ...or really any standard that your development department (or developer) chooses to use.
            foreach (var unknown in unknownConstraintsInDatabase)
            {
                if (foreignKeysInSchema.InvariantContains(unknown) || primaryKeysInSchema.InvariantContains(unknown))
                {
                    result.ValidConstraints.Add(unknown);
                }
                else
                {
                    result.Errors.Add(new Tuple<string, string>("Unknown", unknown));
                }
            }

            //Foreign keys:

            var validForeignKeyDifferences = foreignKeysInDatabase.Intersect(foreignKeysInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var foreignKey in validForeignKeyDifferences)
            {
                result.ValidConstraints.Add(foreignKey);
            }
            var invalidForeignKeyDifferences =
                foreignKeysInDatabase.Except(foreignKeysInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(foreignKeysInSchema.Except(foreignKeysInDatabase, StringComparer.InvariantCultureIgnoreCase));
            foreach (var foreignKey in invalidForeignKeyDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", foreignKey));
            }


            //Primary keys:

            //Add valid and invalid primary key differences to the result object
            var validPrimaryKeyDifferences = primaryKeysInDatabase.Intersect(primaryKeysInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var primaryKey in validPrimaryKeyDifferences)
            {
                result.ValidConstraints.Add(primaryKey);
            }
            var invalidPrimaryKeyDifferences =
                primaryKeysInDatabase.Except(primaryKeysInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(primaryKeysInSchema.Except(primaryKeysInDatabase, StringComparer.InvariantCultureIgnoreCase));
            foreach (var primaryKey in invalidPrimaryKeyDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", primaryKey));
            }

        }

        private void ValidateDbColumns(DatabaseSchemaResult result)
        {
            //Check columns in configured database against columns in schema
            var columnsInDatabase = SqlSyntax.GetColumnsInSchema(_database);
            var columnsPerTableInDatabase = columnsInDatabase.Select(x => string.Concat(x.TableName, ",", x.ColumnName)).ToList();
            var columnsPerTableInSchema = result.TableDefinitions.SelectMany(x => x.Columns.Select(y => string.Concat(y.TableName, ",", y.Name))).ToList();
            //Add valid and invalid column differences to the result object
            var validColumnDifferences = columnsPerTableInDatabase.Intersect(columnsPerTableInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var column in validColumnDifferences)
            {
                result.ValidColumns.Add(column);
            }

            var invalidColumnDifferences =
                columnsPerTableInDatabase.Except(columnsPerTableInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(columnsPerTableInSchema.Except(columnsPerTableInDatabase, StringComparer.InvariantCultureIgnoreCase));
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
            var validTableDifferences = tablesInDatabase.Intersect(tablesInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var tableName in validTableDifferences)
            {
                result.ValidTables.Add(tableName);
            }

            var invalidTableDifferences =
                tablesInDatabase.Except(tablesInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(tablesInSchema.Except(tablesInDatabase, StringComparer.InvariantCultureIgnoreCase));
            foreach (var tableName in invalidTableDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Table", tableName));
            }
        }

        private void ValidateDbIndexes(DatabaseSchemaResult result)
        {
            //These are just column indexes NOT constraints or Keys
            //var colIndexesInDatabase = result.DbIndexDefinitions.Where(x => x.IndexName.InvariantStartsWith("IX_")).Select(x => x.IndexName).ToList();
            var colIndexesInDatabase = result.DbIndexDefinitions.Select(x => x.IndexName).ToList();
            var indexesInSchema = result.TableDefinitions.SelectMany(x => x.Indexes.Select(y => y.Name)).ToList();

            //Add valid and invalid index differences to the result object
            var validColIndexDifferences = colIndexesInDatabase.Intersect(indexesInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var index in validColIndexDifferences)
            {
                result.ValidIndexes.Add(index);
            }

            var invalidColIndexDifferences =
                colIndexesInDatabase.Except(indexesInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(indexesInSchema.Except(colIndexesInDatabase, StringComparer.InvariantCultureIgnoreCase));
            foreach (var index in invalidColIndexDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Index", index));
            }
        }

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        internal delegate void DatabaseEventHandler(DatabaseCreationEventArgs e);

        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        internal static event DatabaseEventHandler BeforeCreation;
        /// <summary>
        /// Raises the <see cref="BeforeCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal virtual void FireBeforeCreation(DatabaseCreationEventArgs e)
        {
            BeforeCreation?.Invoke(e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        internal static event DatabaseEventHandler AfterCreation;
        /// <summary>
        /// Raises the <see cref="AfterCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        internal virtual void FireAfterCreation(DatabaseCreationEventArgs e)
        {
            AfterCreation?.Invoke(e);
        }

        #endregion

        #region Utilities

        public bool TableExists(string tableName)
        {
            return SqlSyntax.DoesTableExist(_database, tableName);
        }

        public bool TableExists<T>()        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            return table != null && TableExists(table.Name);        }

        // this is used in tests
        internal void CreateTable<T>(bool overwrite = false)
            where T : new()
        {
            var tableType = typeof(T);
            CreateTable(overwrite, tableType, new DatabaseDataCreator(_database, _logger));
        }

        public void CreateTable(bool overwrite, Type modelType, DatabaseDataCreator dataCreation)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType, SqlSyntax);
            var tableName = tableDefinition.Name;

            var createSql = SqlSyntax.Format(tableDefinition);
            var createPrimaryKeySql = SqlSyntax.FormatPrimaryKey(tableDefinition);
            var foreignSql = SqlSyntax.Format(tableDefinition.ForeignKeys);
            var indexSql = SqlSyntax.Format(tableDefinition.Indexes);

            var tableExist = TableExists(tableName);
            if (overwrite && tableExist)
            {
                DropTable(tableName);
                tableExist = false;
            }

            if (tableExist == false)
            {
                using (var transaction = _database.GetTransaction())
                {
                    //Execute the Create Table sql
                    var created = _database.Execute(new Sql(createSql));
                    _logger.Info<DatabaseSchemaCreator>("Create Table '{TableName}' ({Created}): \n {Sql}", tableName, created, createSql);

                    //If any statements exists for the primary key execute them here
                    if (string.IsNullOrEmpty(createPrimaryKeySql) == false)
                    {
                        var createdPk = _database.Execute(new Sql(createPrimaryKeySql));
                        _logger.Info<DatabaseSchemaCreator>("Create Primary Key ({CreatedPk}):\n {Sql}", createdPk, createPrimaryKeySql);
                    }

                    //Turn on identity insert if db provider is not mysql
                    if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} ON "));

                    //Call the NewTable-event to trigger the insert of base/default data
                    //OnNewTable(tableName, _db, e, _logger);

                    dataCreation.InitializeBaseData(tableName);

                    //Turn off identity insert if db provider is not mysql
                    if (SqlSyntax.SupportsIdentityInsert() && tableDefinition.Columns.Any(x => x.IsIdentity))
                        _database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(tableName)} OFF;"));

                    //Special case for MySql
                    if (SqlSyntax is MySqlSyntaxProvider && tableName.Equals("umbracoUser"))
                    {
                        _database.Update<UserDto>("SET id = @IdAfter WHERE id = @IdBefore AND userLogin = @Login", new { IdAfter = 0, IdBefore = 1, Login = "admin" });
                    }

                    //Loop through index statements and execute sql
                    foreach (var sql in indexSql)
                    {
                        var createdIndex = _database.Execute(new Sql(sql));
                        _logger.Info<DatabaseSchemaCreator>("Create Index ({CreatedIndex}):\n {Sql}", createdIndex, sql);
                    }

                    //Loop through foreignkey statements and execute sql
                    foreach (var sql in foreignSql)
                    {
                        var createdFk = _database.Execute(new Sql(sql));
                        _logger.Info<DatabaseSchemaCreator>("Create Foreign Key ({CreatedFk}):\n {Sql}", createdFk, sql);
                    }

                    transaction.Complete();
                }
            }

            _logger.Info<DatabaseSchemaCreator>("Created table '{TableName}'", tableName);
        }

        public void DropTable(string tableName)
        {
            var sql = new Sql(string.Format(SqlSyntax.DropTable, SqlSyntax.GetQuotedTableName(tableName)));
            _database.Execute(sql);
        }

        #endregion
    }
}
