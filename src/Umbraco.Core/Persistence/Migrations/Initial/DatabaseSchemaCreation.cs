using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Initial
{
    /// <summary>
    /// Represents the initial database schema creation by running CreateTable for all DTOs against the db.
    /// </summary>
    internal class DatabaseSchemaCreation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="database"></param>
        /// <param name="logger"></param>
        /// <param name="sqlSyntaxProvider"></param>
        public DatabaseSchemaCreation(Database database, ILogger logger, ISqlSyntaxProvider sqlSyntaxProvider)
        {
            _database = database;
            _logger = logger;
            _sqlSyntaxProvider = sqlSyntaxProvider;
             _schemaHelper = new DatabaseSchemaHelper(database, logger, sqlSyntaxProvider);
        }

        #region Private Members

        private readonly DatabaseSchemaHelper _schemaHelper;
        private readonly Database _database;
        private readonly ILogger _logger;
        private readonly ISqlSyntaxProvider _sqlSyntaxProvider;

        private static readonly Dictionary<int, Type> OrderedTables = new Dictionary<int, Type>
                                                                          {
                                                                              {0, typeof (NodeDto)},
                                                                              {1, typeof (ContentTypeDto)},
                                                                              {2, typeof (TemplateDto)},
                                                                              {3, typeof (ContentDto)},
                                                                              {4, typeof (ContentVersionDto)},
                                                                              {5, typeof (DocumentDto)},
                                                                              
                                                                              {6, typeof (DocumentTypeDto)},
                                                                              {7, typeof (DataTypeDto)},
                                                                              {8, typeof (DataTypePreValueDto)},
                                                                              {9, typeof (DictionaryDto)},
                                                                              {10, typeof (LanguageTextDto)},
                                                                              {11, typeof (LanguageDto)},
                                                                              {12, typeof (DomainDto)},
                                                                              {13, typeof (LogDto)},
                                                                              {14, typeof (MacroDto)},
                                                                              {15, typeof (MacroPropertyDto)},
                                                                              {16, typeof (MemberTypeDto)},
                                                                              {17, typeof (MemberDto)},
                                                                              {18, typeof (Member2MemberGroupDto)},
                                                                              {19, typeof (ContentXmlDto)},
                                                                              {20, typeof (PreviewXmlDto)},
                                                                              {21, typeof (PropertyTypeGroupDto)},
                                                                              {22, typeof (PropertyTypeDto)},
                                                                              {23, typeof (PropertyDataDto)},
                                                                              {24, typeof (RelationTypeDto)},
                                                                              {25, typeof (RelationDto)},
                                                                              {26, typeof (StylesheetDto)},
                                                                              {27, typeof (StylesheetPropertyDto)},
                                                                              {28, typeof (TagDto)},
                                                                              {29, typeof (TagRelationshipDto)},
                                                                              {30, typeof (UserLoginDto)},
                                                                              {31, typeof (UserTypeDto)},
                                                                              {32, typeof (UserDto)},
                                                                              {33, typeof (TaskTypeDto)},
                                                                              {34, typeof (TaskDto)},
                                                                              {35, typeof (ContentType2ContentTypeDto)},
                                                                              {36, typeof (ContentTypeAllowedContentTypeDto)},
                                                                              {37, typeof (User2AppDto)},
                                                                              {38, typeof (User2NodeNotifyDto)},
                                                                              {39, typeof (User2NodePermissionDto)},
                                                                              {40, typeof (ServerRegistrationDto)},
                                                                              {41, typeof (AccessDto)},
                                                                              {42, typeof (AccessRuleDto)},
                                                                              {43, typeof(CacheInstructionDto)}
                                                                          };
        #endregion
        
        /// <summary>
        /// Drops all Umbraco tables in the db
        /// </summary>
        internal void UninstallDatabaseSchema()
        {
            _logger.Info<DatabaseSchemaCreation>("Start UninstallDatabaseSchema");

            foreach (var item in OrderedTables.OrderByDescending(x => x.Key))
            {
                var tableNameAttribute = item.Value.FirstAttribute<TableNameAttribute>();

                string tableName = tableNameAttribute == null ? item.Value.Name : tableNameAttribute.Value;

                _logger.Info<DatabaseSchemaCreation>("Uninstall" + tableName);

                try
                {
                    if (_schemaHelper.TableExist(tableName))
                    {
                        _schemaHelper.DropTable(tableName);    
                    }
                }
                catch (Exception ex)
                {
                    //swallow this for now, not sure how best to handle this with diff databases... though this is internal
                    // and only used for unit tests. If this fails its because the table doesn't exist... generally!
                    _logger.Error<DatabaseSchemaCreation>("Could not drop table " + tableName, ex);
                }
            }
        }

       

        /// <summary>
        /// Initialize the database by creating the umbraco db schema
        /// </summary>
        public void InitializeDatabaseSchema()
        {
            var e = new DatabaseCreationEventArgs();
            FireBeforeCreation(e);

            if (!e.Cancel)
            {
                foreach (var item in OrderedTables.OrderBy(x => x.Key))
                {
                    _schemaHelper.CreateTable(false, item.Value);
                }
            }

            FireAfterCreation(e);
        }

        /// <summary>
        /// Validates the schema of the current database
        /// </summary>
        public DatabaseSchemaResult ValidateSchema()
        {
            var result = new DatabaseSchemaResult();

            //get the db index defs
            result.DbIndexDefinitions = _sqlSyntaxProvider.GetDefinedIndexes(_database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            foreach (var item in OrderedTables.OrderBy(x => x.Key))
            {
                var tableDefinition = DefinitionFactory.GetTableDefinition(item.Value);
                result.TableDefinitions.Add(tableDefinition);
            }

            ValidateDbTables(result);

            ValidateDbColumns(result);

            ValidateDbIndexes(result);

            ValidateDbConstraints(result);

            return result;
        }

        private void ValidateDbConstraints(DatabaseSchemaResult result)
        {
            //MySql doesn't conform to the "normal" naming of constraints, so there is currently no point in doing these checks.
            //TODO: At a later point we do other checks for MySql, but ideally it should be necessary to do special checks for different providers.
            // ALso note that to get the constraints for MySql we have to open a connection which we currently have not.
            if (_sqlSyntaxProvider is MySqlSyntaxProvider)
                return;

            //Check constraints in configured database against constraints in schema
            var constraintsInDatabase = _sqlSyntaxProvider.GetConstraintsPerColumn(_database).DistinctBy(x => x.Item3).ToList();
            var foreignKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("FK_")).Select(x => x.Item3).ToList();
            var primaryKeysInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("PK_")).Select(x => x.Item3).ToList();
            var indexesInDatabase = constraintsInDatabase.Where(x => x.Item3.InvariantStartsWith("IX_")).Select(x => x.Item3).ToList();
            var indexesInSchema = result.TableDefinitions.SelectMany(x => x.Indexes.Select(y => y.Name)).ToList();
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
                if (foreignKeysInSchema.InvariantContains(unknown) || primaryKeysInSchema.InvariantContains(unknown) || indexesInSchema.InvariantContains(unknown))
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

            //Constaints:

            //NOTE: SD: The colIndex checks above should really take care of this but I need to keep this here because it was here before
            // and some schema validation checks might rely on this data remaining here!
            //Add valid and invalid index differences to the result object
            var validIndexDifferences = indexesInDatabase.Intersect(indexesInSchema, StringComparer.InvariantCultureIgnoreCase);
            foreach (var index in validIndexDifferences)
            {
                result.ValidConstraints.Add(index);
            }
            var invalidIndexDifferences =
                indexesInDatabase.Except(indexesInSchema, StringComparer.InvariantCultureIgnoreCase)
                                .Union(indexesInSchema.Except(indexesInDatabase, StringComparer.InvariantCultureIgnoreCase));
            foreach (var index in invalidIndexDifferences)
            {
                result.Errors.Add(new Tuple<string, string>("Constraint", index));
            }
        }

        private void ValidateDbColumns(DatabaseSchemaResult result)
        {
            //Check columns in configured database against columns in schema
            var columnsInDatabase = _sqlSyntaxProvider.GetColumnsInSchema(_database);
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
            var tablesInDatabase = _sqlSyntaxProvider.GetTablesInSchema(_database).ToList();
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
        protected internal virtual void FireBeforeCreation(DatabaseCreationEventArgs e)
        {
            if (BeforeCreation != null)
            {
                BeforeCreation(e);
            }
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        internal static event DatabaseEventHandler AfterCreation;
        /// <summary>
        /// Raises the <see cref="AfterCreation"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterCreation(DatabaseCreationEventArgs e)
        {
            if (AfterCreation != null)
            {
                AfterCreation(e);
            }
        }

        #endregion
    }
}