using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

// TODO (V18): Remove the default implementations in this interface.

/// <summary>
///     Defines an SqlSyntaxProvider.
/// </summary>
public interface ISqlSyntaxProvider
{
    /// <summary>
    /// Gets the SQL syntax expression used to calculate the length of a string in the current SQL dialect.
    /// </summary>
    string Length { get; }

    /// <summary>
    /// Gets the SQL syntax representation for the substring function.
    /// </summary>
    string Substring { get; }

    /// <summary>
    /// Gets the name that identifies the SQL syntax provider implementation.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the SQL statement or syntax used to create a table in the database.
    /// </summary>
    string CreateTable { get; }

    /// <summary>
    /// Gets the SQL statement template used to drop a table in the database.
    /// </summary>
    string DropTable { get; }

    /// <summary>
    /// Gets the SQL syntax statement or template used for adding a column to a table.
    /// </summary>
    string AddColumn { get; }

    /// <summary>
    /// Gets the SQL syntax statement used to drop a column from a table.
    /// </summary>
    string DropColumn { get; }

    /// <summary>
    /// Gets the SQL syntax template or statement used to alter a column in a database table.
    /// </summary>
    string AlterColumn { get; }

    /// <summary>
    /// Gets the SQL syntax statement or template used for renaming a column in a database table.
    /// </summary>
    string RenameColumn { get; }

    /// <summary>
    /// Gets the SQL syntax statement used to rename a table in the database.
    /// </summary>
    string RenameTable { get; }

    /// <summary>
    /// Gets the SQL statement used to create a database schema.
    /// </summary>
    string CreateSchema { get; }

    /// <summary>
    /// Gets the SQL syntax template or statement used for altering a database schema.
    /// </summary>
    string AlterSchema { get; }

    /// <summary>
    /// Gets the SQL statement or syntax template used to drop a database schema.
    /// </summary>
    string DropSchema { get; }

    /// <summary>
    /// Gets the SQL statement template used for creating an index in the database.
    /// </summary>
    string CreateIndex { get; }

    /// <summary>
    /// Gets the SQL syntax template used to generate a statement for dropping an index.
    /// </summary>
    string DropIndex { get; }

    /// <summary>
    /// Gets the SQL syntax template used for inserting data into a database table.
    /// </summary>
    string InsertData { get; }

    /// <summary>
    /// Gets the SQL syntax template used for constructing an UPDATE statement to modify data in a table.
    /// </summary>
    string UpdateData { get; }

    /// <summary>
    /// Gets the SQL syntax statement or template used for deleting data from a database.
    /// </summary>
    string DeleteData { get; }

    /// <summary>
    /// Gets the SQL statement used to truncate (remove all rows from) a table.
    /// </summary>
    string TruncateTable { get; }

    /// <summary>
    /// Gets the SQL syntax statement used to create a database constraint.
    /// </summary>
    string CreateConstraint { get; }

    /// <summary>
    /// Gets the SQL syntax statement used to delete a database constraint.
    /// </summary>
    string DeleteConstraint { get; }

    /// <summary>
    /// Gets the SQL syntax statement used to delete a default constraint from a database column.
    /// </summary>
    string DeleteDefaultConstraint { get; }

    /// <summary>
    ///     Gets a regex matching aliased fields.
    /// </summary>
    /// <remarks>
    ///     <para>Matches "(table.column) AS (alias)" where table, column and alias are properly escaped.</para>
    /// </remarks>
    Regex AliasRegex { get; }

    /// <summary>
    /// Gets a string representation of an integer that preserves its numeric order when sorted lexicographically.
    /// </summary>
    string ConvertIntegerToOrderableString { get; }

    /// <summary>
    /// Gets a string representation of a date that preserves chronological order when sorted in SQL queries.
    /// </summary>
    string ConvertDateToOrderableString { get; }

    /// <summary>
    /// Gets a string representation of a decimal value that preserves numeric ordering when used in SQL queries.
    /// </summary>
    string ConvertDecimalToOrderableString { get; }

    /// <summary>
    /// Converts a unique identifier (such as a GUID) to its string representation for use in SQL statements.
    /// </summary>
    string ConvertUniqueIdentifierToString => throw new NotImplementedException();

    /// <summary>
    /// Converts the specified integer value to its equivalent boolean representation in SQL syntax.
    /// </summary>
    /// <param name="value">The integer value to convert (typically 0 or 1).</param>
    /// <returns>A string containing the SQL representation of the boolean value.</returns>
    string ConvertIntegerToBoolean(int value);

    /// <summary>
    ///     Returns the default isolation level for the database
    /// </summary>
    IsolationLevel DefaultIsolationLevel { get; }

    /// <summary>
    /// Gets the identifier or name of the underlying database provider used by this SQL syntax provider.
    /// </summary>
    string DbProvider { get; }

    /// <summary>
    /// Gets the dictionary of scalar mappers used by the SQL syntax provider.
    /// Scalar mappers are responsible for converting database scalar values to .NET types during data retrieval operations.
    /// The dictionary maps .NET types to their corresponding <see cref="IScalarMapper"/> implementations.
    /// </summary>
    IDictionary<Type, IScalarMapper>? ScalarMappers => null;

    /// <summary>
    /// Determines the appropriate database type based on the specified current type and an optional connection string.
    /// </summary>
    /// <param name="current">The current <see cref="DatabaseType"/>.</param>
    /// <param name="connectionString">An optional connection string used to help determine the updated database type.</param>
    /// <returns>The resolved <see cref="Umbraco.Cms.Infrastructure.Persistence.DatabaseType"/>.</returns>
    DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString) =>
        current; // Default implementation.

    /// <summary>
    /// Escapes a string value for safe inclusion in SQL queries, helping to prevent SQL injection attacks.
    /// </summary>
    /// <param name="val">The string value to escape.</param>
    /// <returns>The escaped string safe for use in SQL queries.</returns>
    string EscapeString(string val);

    /// <summary>
    /// Returns the SQL wildcard placeholder string used for parameterized LIKE queries.
    /// </summary>
    /// <returns>The SQL wildcard placeholder string (e.g., "%" or "*").</returns>
    string GetWildcardPlaceholder();

    /// <summary>
    /// This ensures that GetWildcardPlaceholder() character is surronded by '' when used inside a LIKE statement. E.g. in WhereLike() extension and the defaultConcat is used.
    /// </summary>
    /// <param name="concatDefault">When provided this overides the GetWildcardPlaceholder() default.</param>
    /// <returns></returns>
    string GetWildcardConcat(string concatDefault = "");

    /// <summary>
    /// Constructs a SQL expression that compares a string column to a parameter value for equality, using the appropriate syntax for the specified text column type.
    /// </summary>
    /// <param name="column">The name of the string column to compare.</param>
    /// <param name="paramIndex">The index of the parameter placeholder to use in the SQL statement.</param>
    /// <param name="columnType">The type of the text column, which may affect how the comparison is performed (e.g., case sensitivity or collation).</param>
    /// <returns>A SQL string representing the equality comparison between the column and the parameter.</returns>
    string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType);

    /// <summary>
    /// Constructs a SQL WHERE clause fragment for performing a wildcard comparison on a specified text column.
    /// </summary>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramIndex">The index of the parameter to use in the comparison (for parameterized queries).</param>
    /// <param name="columnType">The type of the text column, which may affect how the comparison is constructed.</param>
    /// <returns>A string representing the SQL WHERE clause fragment for a wildcard comparison on the specified column.</returns>
    string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType);

    /// <summary>Concatenates the specified string arguments into a single string using the SQL syntax appropriate for the database.</summary>
    /// <param name="args">The string arguments to concatenate.</param>
    /// <returns>A string representing the SQL concatenation of the arguments.</returns>
    string GetConcat(params string[] args);

    /// <summary>
    /// Returns the SQL representation of a column for a given database type, table, and column name, with optional aliasing and reference for joins.
    /// </summary>
    /// <param name="dbType">The database type (e.g., SQL Server, MySQL).</param>
    /// <param name="tableName">The name of the table containing the column.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="columnAlias">An optional alias to use for the column in the SQL statement.</param>
    /// <param name="referenceName">An optional reference name, typically used as a table alias in join scenarios.</param>
    /// <param name="forInsert">True if the column is being used in an INSERT statement; otherwise, false.</param>
    /// <returns>A SQL string representing the column, formatted appropriately for the specified database type and context.</returns>
    string GetColumn(DatabaseType dbType, string tableName, string columnName, string? columnAlias, string? referenceName = null, bool forInsert = false);

    /// <summary>
    /// Returns the specified table name properly quoted for use in SQL statements, according to the SQL dialect.
    /// </summary>
    /// <param name="tableName">The name of the table to be quoted. Can be <c>null</c>.</param>
    /// <returns>The quoted table name as a <see cref="string"/>.</returns>
    string GetQuotedTableName(string? tableName);

    /// <summary>
    /// Returns the specified column name with appropriate SQL identifier quoting applied, according to the current SQL dialect.
    /// </summary>
    /// <param name="columnName">The name of the column to be quoted. Can be <c>null</c>.</param>
    /// <returns>The quoted column name as a string.</returns>
    string GetQuotedColumnName(string? columnName);

    /// <summary>
    /// Generates the SQL ORDER BY clause for a GUID column in the specified table.
    /// </summary>
    /// <param name="tableName">The name of the table containing the GUID column.</param>
    /// <param name="columnName">The name of the GUID column to order by.</param>
    /// <returns>A SQL string representing the ORDER BY clause for the specified GUID column.</returns>
    string OrderByGuid(string tableName, string columnName);

    /// <summary>
    /// Returns the specified SQL identifier name properly quoted for use in SQL queries, according to the syntax rules of the underlying database.
    /// </summary>
    /// <param name="name">The SQL identifier name to quote. May be <c>null</c>.</param>
    /// <returns>The quoted name as a string, or <c>null</c> if <paramref name="name"/> is <c>null</c>.</returns>
    string GetQuotedName(string? name);

    /// <summary>
    /// Gets the SQL type cast extension (null type annotation) associated with a null value for the specified type parameter.
    /// </summary>
    /// <remarks>
    /// This method is useful when generating SQL queries that require explicit type casting of NULL values,
    /// such as in PostgreSQL. The returned string can be used directly in SQL statements for type-safe
    /// comparisons or assignments (for example, <c>::integer</c> or <c>::text</c>).
    /// </remarks>
    /// <typeparam name="T">The type for which to retrieve the SQL null type annotation.</typeparam>
    /// <returns>
    /// A string containing the SQL type cast extension (null type annotation) that represents a null value for type
    /// <typeparamref name="T"/>, or an empty string if no extension is defined.
    /// </returns>
    string GetNullCastSuffix<T>() => string.Empty;

    /// <summary>
    /// Determines whether the specified table exists in the database.
    /// </summary>
    /// <param name="db">The database instance to check the table in.</param>
    /// <param name="tableName">The name of the table to check for existence.</param>
    /// <returns>True if the table exists; otherwise, false.</returns>
    bool DoesTableExist(IDatabase db, string tableName);

    /// <summary>
    /// Returns the SQL keyword or expression that represents the specified index type.
    /// </summary>
    /// <param name="indexTypes">The <see cref="IndexTypes"/> value specifying the type of index.</param>
    /// <returns>A string containing the SQL representation of the index type.</returns>
    string GetIndexType(IndexTypes indexTypes);

    /// <summary>
    /// Returns the SQL type declaration string corresponding to the specified <see cref="SpecialDbType"/>.
    /// </summary>
    /// <param name="dbType">The special database type for which to retrieve the SQL type declaration.</param>
    /// <returns>A string containing the SQL type declaration for the given special database type.</returns>
    string GetSpecialDbType(SpecialDbType dbType);

    /// <summary>
    /// Formats a <see cref="DateTime"/> value as a string suitable for use in SQL queries.
    /// </summary>
    /// <param name="date">The date and time value to format.</param>
    /// <param name="includeTime">If <c>true</c>, includes the time component; otherwise, only the date is included.</param>
    /// <returns>A string representation of the date (and optionally time) formatted for SQL.</returns>
    string FormatDateTime(DateTime date, bool includeTime = true);

    /// <summary>
    /// Formats a <see cref="TableDefinition"/> into its corresponding SQL statement.
    /// </summary>
    /// <param name="table">The <see cref="TableDefinition"/> to format.</param>
    /// <returns>A SQL string representing the table definition.</returns>
    string Format(TableDefinition table);

    /// <summary>
    /// Formats a collection of <see cref="ColumnDefinition"/> objects into their SQL string representation.
    /// </summary>
    /// <param name="columns">The collection of columns to format.</param>
    /// <returns>A SQL string representing the formatted columns.</returns>
    string Format(IEnumerable<ColumnDefinition> columns);

    /// <summary>
    /// Formats a collection of <see cref="IndexDefinition"/> objects into their corresponding SQL statements.
    /// </summary>
    /// <param name="indexes">The collection of index definitions to be formatted.</param>
    /// <returns>A list of SQL strings representing the formatted index definitions.</returns>
    List<string> Format(IEnumerable<IndexDefinition> indexes);

    /// <summary>
    /// Formats a collection of foreign key definitions into their corresponding SQL statements.
    /// </summary>
    /// <param name="foreignKeys">The foreign key definitions to format.</param>
    /// <returns>A list of SQL strings representing the formatted foreign keys.</returns>
    List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys);

    /// <summary>
    /// Generates the SQL statement for defining the primary key of the specified table.
    /// </summary>
    /// <param name="table">The <see cref="TableDefinition"/> representing the table whose primary key definition is to be formatted.</param>
    /// <returns>A string containing the SQL definition for the primary key.</returns>
    string FormatPrimaryKey(TableDefinition table);

    /// <summary>
    /// Returns the specified string value properly quoted for safe inclusion in an SQL statement.
    /// </summary>
    /// <param name="value">The string value to be quoted.</param>
    /// <returns>The input string value wrapped in appropriate SQL quotes.</returns>
    string GetQuotedValue(string value);

    /// <summary>
    /// Formats the specified <see cref="ColumnDefinition"/> into its SQL representation.
    /// </summary>
    /// <param name="column">The <see cref="ColumnDefinition"/> to format.</param>
    /// <returns>A SQL string that defines the column according to the provider's syntax.</returns>
    string Format(ColumnDefinition column);

    /// <summary>
    /// Formats a <see cref="ColumnDefinition"/> into its corresponding SQL statement for the specified table.
    /// </summary>
    /// <param name="column">The <see cref="ColumnDefinition"/> to format.</param>
    /// <param name="tableName">The name of the table the column belongs to.</param>
    /// <param name="sqls">Outputs additional SQL statements required for the column definition.</param>
    /// <returns>A SQL string representing the column definition.</returns>
    string Format(ColumnDefinition column, string tableName, out IEnumerable<string> sqls);

    /// <summary>
    /// Formats the specified <see cref="IndexDefinition"/> into its corresponding SQL statement.
    /// </summary>
    /// <param name="index">The <see cref="IndexDefinition"/> to format.</param>
    /// <returns>A SQL string that defines the specified index.</returns>
    string Format(IndexDefinition index);

    /// <summary>Formats the specified foreign key definition into a SQL string.</summary>
    /// <param name="foreignKey">The foreign key definition to format.</param>
    /// <returns>A SQL string representing the foreign key definition.</returns>
    string Format(ForeignKeyDefinition foreignKey);

    /// <summary>
    /// Formats the SQL syntax for renaming a column in a table.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column to rename.</param>
    /// <param name="oldName">The current name of the column.</param>
    /// <param name="newName">The new name for the column.</param>
    /// <returns>A SQL string that performs the column rename operation.</returns>
    string FormatColumnRename(string? tableName, string? oldName, string? newName);

    /// <summary>
    /// Formats the SQL statement to rename a table from an old name to a new name.
    /// </summary>
    /// <param name="oldName">The current name of the table.</param>
    /// <param name="newName">The new name for the table.</param>
    /// <returns>A SQL string that performs the table rename operation.</returns>
    string FormatTableRename(string? oldName, string? newName);

    /// <summary>
    /// Returns a column name with an alias, optionally prefixed by a table name or alias.
    /// </summary>
    /// <param name="tableNameOrAlias">The table name or alias to prefix the column with.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="columnAlias">The alias to assign to the column.</param>
    /// <returns>A string representing the column with its alias, optionally prefixed by the table name or alias.</returns>
    string ColumnWithAlias(string tableNameOrAlias, string columnName, string columnAlias = "");

    /// <summary>
    /// Creates a table in the specified database using the provided table definition.
    /// </summary>
    /// <param name="database">The <see cref="IDatabase"/> instance where the table will be created.</param>
    /// <param name="tableDefinition">The <see cref="TableDefinition"/> describing the table schema.</param>
    /// <param name="skipKeysAndIndexes">If set to <c>true</c>, skips the creation of keys and indexes for the table.</param>
    void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false);

    /// <summary>
    /// Modifies the given SQL query to limit the result set to the specified number of rows.
    /// </summary>
    /// <param name="sql">The SQL query to be modified.</param>
    /// <param name="top">The maximum number of rows to return.</param>
    /// <returns>The modified SQL query limited to the specified number of rows.</returns>
    Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top);

    /// <summary>
    /// Determines whether the SQL syntax provider supports clustered indexes.
    /// </summary>
    /// <returns><c>true</c> if clustered indexes are supported; otherwise, <c>false</c>.</returns>
    bool SupportsClustered();

    /// <summary>
    /// Determines whether the SQL syntax provider supports identity insert operations.
    /// </summary>
    /// <returns><c>true</c> if identity insert is supported; otherwise, <c>false</c>.</returns>
    bool SupportsIdentityInsert();

    /// <summary>
    /// Determines whether the current database provider supports sequence objects for generating numeric values like PostgreSQL.
    /// </summary>
    /// <returns>true if the provider supports sequences; otherwise, false.</returns>
    bool SupportsSequences() => false;

    /// <summary>
    /// Alters the database sequences to match the current schema requirements.
    /// </summary>
    /// <remarks>
    /// This is an optional extension point for SQL providers that support database sequences. Providers that support
    /// sequences should override this method and implement any required changes when schema updates (for example, after
    /// a migration) require sequence adjustments. Callers should typically check <see cref="SupportsSequences"/> before
    /// invoking this method. The default implementation throws <see cref="NotImplementedException"/>.
    /// </remarks>
    /// <param name="database">The database connection to use for altering sequences.</param>
    void AlterSequences(IUmbracoDatabase database) => throw new NotImplementedException();

    /// <summary>
    /// Alters the database sequences associated with the specified table for providers that support sequences.
    /// </summary>
    /// <remarks>
    /// This is an optional extension point for SQL providers that support database sequences. Providers that support
    /// sequences should override this method to update sequences associated with the specified table when schema changes
    /// require it. Callers should typically check <see cref="SupportsSequences"/> before invoking this method. The default
    /// implementation throws <see cref="NotImplementedException"/>.
    /// </remarks>
    /// <param name="database">The database connection to use for altering the sequences.</param>
    /// <param name="tableName">The name of the table whose sequences will be altered.</param>
    void AlterSequences(IUmbracoDatabase database, string tableName) => throw new NotImplementedException();

    /// <summary>
    /// Returns the names of all tables in the schema for the specified database.
    /// </summary>
    /// <param name="db">The database from which to retrieve table names.</param>
    /// <returns>An enumerable collection of table names in the schema.</returns>
    IEnumerable<string> GetTablesInSchema(IDatabase db);

    /// <summary>
    /// Retrieves information about all columns defined in the schema of the specified database.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> instance representing the database to inspect.</param>
    /// <returns>An <see cref="IEnumerable{ColumnInfo}"/> containing details for each column in the database schema.</returns>
    IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db);

    /// <summary>
    ///     Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not
    ///     include indexes)
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, ConstraintName
    /// </returns>
    IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db);

    /// <summary>
    ///     Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not
    ///     include indexes)
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, ColumnName, ConstraintName
    /// </returns>
    IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db);

    /// <summary>
    ///     Returns all defined Indexes in the database excluding primary keys
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, IndexName, ColumnName, IsUnique
    /// </returns>
    IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db);

    /// <summary>
    ///     Tries to gets the name of the default constraint on a column.
    /// </summary>
    /// <param name="db">The database.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <returns>A value indicating whether a default constraint was found.</returns>
    /// <remarks>
    ///     <para>
    ///         Some database engines may not have names for default constraints,
    ///         in which case the function may return true, but <paramref name="constraintName" /> is
    ///         unspecified.
    ///     </para>
    /// </remarks>
    bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName);

    /// <summary>
    /// Determines whether the specified primary key exists on the given table.
    /// </summary>
    /// <param name="db">The database instance to use for the check.</param>
    /// <param name="tableName">The name of the table to check.</param>
    /// <param name="primaryKeyName">The name of the primary key to look for.</param>
    /// <returns>True if the primary key exists; otherwise, false.</returns>
    bool DoesPrimaryKeyExist(IDatabase db, string tableName, string primaryKeyName) => throw new NotImplementedException();

    /// <summary>
    /// Returns the formatted field name to be used in an SQL UPDATE statement for the specified field selector.
    /// </summary>
    /// <typeparam name="TDto">The type of the data transfer object (DTO) containing the field.</typeparam>
    /// <param name="fieldSelector">An expression selecting the field from the DTO.</param>
    /// <param name="tableAlias">An optional table alias to prefix the field name; if <c>null</c>, no alias is used.</param>
    /// <returns>The formatted field name, suitable for use in an UPDATE statement.</returns>
    string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object?>> fieldSelector, string? tableAlias = null);

    /// <summary>
    /// Appends the appropriate FOR UPDATE hint to the specified SQL query, if applicable for the database dialect.
    /// </summary>
    /// <param name="sql">The SQL query to which the FOR UPDATE hint will be appended.</param>
    /// <returns>The modified SQL query with the FOR UPDATE hint appended, or the original query if not applicable.</returns>
    Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql);

    /// <summary>
    /// Appends the relevant FOR UPDATE hint to the specified SQL query, if applicable for the underlying database.
    /// </summary>
    /// <param name="sql">The SQL query to which the FOR UPDATE hint will be appended.</param>
    /// <returns>A <see cref="Sql{ISqlContext}"/> instance with the FOR UPDATE hint appended, if supported.</returns>
    Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql);

    /// <summary>
    ///     Handles left join with nested join
    /// </summary>
    Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
        string? alias = null);

    /// <summary>
    /// Some databases have a maximum length for constraint names, this method truncates the name if necessary.
    /// </summary>
    /// <typeparam name="T">type of the entity.</typeparam>
    /// <param name="constraintName">unlimited name.</param>
    /// <returns>truncated name.</returns>
    string TruncateConstraintName<T>(string constraintName) => constraintName;
}
