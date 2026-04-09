// Don't remove the unused System using, for some reason this breaks docfx, and I have no clue why.
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
///     Represents the Base Sql Syntax provider implementation.
/// </summary>
/// <remarks>
///     All Sql Syntax provider implementations should derive from this abstract class.
/// </remarks>
/// <typeparam name="TSyntax"></typeparam>
public abstract class SqlSyntaxProviderBase<TSyntax> : ISqlSyntaxProvider
    where TSyntax : ISqlSyntaxProvider
{
    private readonly Lazy<DbTypes> _dbTypes;

    protected SqlSyntaxProviderBase()
    {
        ClauseOrder = new List<Func<ColumnDefinition, string>>
        {
            FormatString,
            FormatType,
            FormatNullable,
            FormatConstraint,
            FormatDefaultValue,
            FormatPrimaryKey,
            FormatIdentity
        };

        //defaults for all providers
        StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
        StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);
        DecimalColumnDefinition =
            string.Format(DecimalColumnDefinitionFormat, DefaultDecimalPrecision, DefaultDecimalScale);

        // ReSharper disable VirtualMemberCallInConstructor
        // ok to call virtual GetQuotedXxxName here - they don't depend on any state
        var col = Regex.Escape(GetQuotedColumnName("column")).Replace("column", @"\w+");
        var fld = Regex.Escape(GetQuotedTableName("table") + ".").Replace("table", @"\w+") + col;
        // ReSharper restore VirtualMemberCallInConstructor
        AliasRegex = new Regex(
            "(" + fld + @")\s+AS\s+(" + col + ")",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        _dbTypes = new Lazy<DbTypes>(InitColumnTypeMap);
    }

    /// <summary>
    /// Gets the format string used to define a non-Unicode string column with a specified length in SQL (e.g., "VARCHAR({0})").
    /// </summary>
    public string StringLengthNonUnicodeColumnDefinitionFormat { get; } = "VARCHAR({0})";

    /// <summary>
    /// Gets the format string used to define a Unicode string column with a specified length.
    /// </summary>
    public virtual string StringLengthUnicodeColumnDefinitionFormat { get; } = "NVARCHAR({0})";

    /// <summary>
    /// Gets the format string used to define a decimal column in SQL, with placeholders for precision and scale.
    /// </summary>
    public string DecimalColumnDefinitionFormat { get; } = "DECIMAL({0},{1})";

    /// <summary>
    /// Gets the format string used to represent default values in SQL statements.
    /// </summary>
    public string DefaultValueFormat { get; } = "DEFAULT ({0})";

    /// <summary>
    /// Gets the default length for string columns.
    /// </summary>
    public int DefaultStringLength { get; } = 255;

    /// <summary>
    /// Gets the default precision (number of digits) used for decimal columns in SQL schemas.
    /// </summary>
    public int DefaultDecimalPrecision { get; } = 20;

    /// <summary>
    /// Gets the default scale (number of decimal places) used for decimal columns in the database.
    /// </summary>
    public int DefaultDecimalScale { get; } = 9;

    /// <summary>
    /// Gets the SQL definition string used for defining a string column in a database schema.
    /// This is typically used when generating or altering database tables.
    /// </summary>
    /// <remarks>Set by Constructor</remarks>
    public virtual string StringColumnDefinition { get; }

    /// <summary>
    /// Gets the format string used for defining a SQL column with a specified string length in the database schema.
    /// </summary>
    public string StringLengthColumnDefinitionFormat { get; }

    /// <summary>
    /// Gets the SQL keyword or clause used to define an auto-incrementing column in a table schema.
    /// </summary>
    public string AutoIncrementDefinition { get; protected set; } = "AUTOINCREMENT";

    /// <summary>
    /// Gets the SQL column definition string used for integer columns.
    /// </summary>
    public string IntColumnDefinition { get; protected set; } = "INTEGER";

    /// <summary>
    /// Gets the SQL column definition string used for representing a 64-bit integer (long) in the database schema.
    /// </summary>
    public string LongColumnDefinition { get; protected set; } = "BIGINT";

    /// <summary>
    /// Gets the SQL column definition string used to define a GUID (Globally Unique Identifier) column in the database schema for the current SQL syntax provider.
    /// </summary>
    public string GuidColumnDefinition { get; protected set; } = "GUID";

    /// <summary>
    /// Gets the SQL type definition string used for boolean columns in the database.
    /// </summary>
    public string BoolColumnDefinition { get; protected set; } = "BOOL";

    /// <summary>
    /// Gets the SQL column definition string for a real (floating-point) data type.
    /// This is typically used when generating SQL for columns of type 'REAL' or 'DOUBLE'.
    /// </summary>
    public string RealColumnDefinition { get; protected set; } = "DOUBLE";

    /// <summary>
    /// Gets or sets the SQL syntax string that defines the structure of a decimal column for the current SQL dialect.
    /// </summary>
    public string DecimalColumnDefinition { get; protected set; }

    /// <summary>
    /// Gets the SQL column definition string used for BLOB (Binary Large Object) data types in the database schema.
    /// </summary>
    public string BlobColumnDefinition { get; protected set; } = "BLOB";

    /// <summary>
    /// Gets the SQL column definition string used for DateTime columns in the database schema.
    /// This typically specifies the SQL data type for storing date and time values.
    /// </summary>
    public string DateTimeColumnDefinition { get; protected set; } = "DATETIME";

    /// <summary>
    /// Gets the SQL column definition string used for storing <see cref="DateTimeOffset"/> values in the database.
    /// </summary>
    public string DateTimeOffsetColumnDefinition { get; protected set; } = "DATETIMEOFFSET(7)";

    /// <summary>
    /// Gets the SQL data type definition used for time columns in the database.
    /// </summary>
    public string TimeColumnDefinition { get; protected set; } = "DATETIME";

    /// <summary>
    /// Gets the SQL column definition string used for columns that store date-only values (without time).
    /// </summary>
    public string DateOnlyColumnDefinition { get; protected set; } = "DATE";

    /// <summary>
    /// Gets the SQL column definition string used for columns that store time-only values (without date component).
    /// </summary>

    public string TimeOnlyColumnDefinition { get; protected set; } = "TIME";

    protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; }

    protected DbTypes DbTypeMap => _dbTypes.Value;

    /// <summary>
    /// Gets the SQL format string used to create a foreign key constraint, with placeholders for table and column names.
    /// </summary>
    public virtual string CreateForeignKeyConstraint =>
        "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}";

    /// <summary>
    /// Gets the SQL statement template used to create a default constraint on a specified column in a table.
    /// The template includes placeholders for the table name, constraint name, default value, and column name.
    /// </summary>
    public virtual string CreateDefaultConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT ({2}) FOR {3}";

    /// <summary>
    /// Gets the regular expression used to match and validate SQL alias identifiers.
    /// </summary>
    public Regex AliasRegex { get; }

    /// <summary>
    /// Gets the SQL wildcard placeholder string ("%") used for parameterized LIKE queries.
    /// </summary>
    /// <returns>The SQL wildcard character used as a placeholder in LIKE queries.</returns>
    public string GetWildcardPlaceholder() => "%";

    /// <summary>
    /// Returns a string suitable for use as a SQL wildcard pattern.
    /// If <paramref name="concatDefault"/> is null or empty, returns the wildcard placeholder wrapped in single quotes.
    /// If <paramref name="concatDefault"/> is not wrapped in single quotes, trims it and wraps it in single quotes.
    /// If <paramref name="concatDefault"/> is already wrapped in single quotes, returns it as is.
    /// </summary>
    /// <param name="concatDefault">The default string to concatenate with the wildcard placeholder, optionally wrapped in single quotes.</param>
    /// <returns>A string representing the wildcard pattern, properly quoted for SQL.</returns>
    public virtual string GetWildcardConcat(string concatDefault = "")
    {
        if (string.IsNullOrEmpty(concatDefault))
        {
            return $"'{GetWildcardPlaceholder()}'";
        }

        if (!concatDefault.StartsWith('\'') || !concatDefault.EndsWith('\''))
        {
            return $"'{concatDefault.Trim()}'";
        }

        return concatDefault;
    }

    /// <summary>
    /// Determines the database type to use, potentially updating it based on the provided connection string.
    /// </summary>
    /// <param name="current">The current <see cref="Umbraco.Cms.Infrastructure.Persistence.DatabaseType"/>.</param>
    /// <param name="connectionString">An optional connection string that may influence the database type.</param>
    /// <returns>The resulting <see cref="Umbraco.Cms.Infrastructure.Persistence.DatabaseType"/>.</returns>
    public virtual DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString) => current;

    /// <summary>
    /// Gets the name that identifies the specific SQL syntax provider implementation.
    /// </summary>
    public abstract string ProviderName { get; }

    /// <summary>
    /// Escapes a string value for safe inclusion in SQL queries by replacing single quotes with doubled single quotes and escaping at symbols ("@").
    /// </summary>
    /// <param name="val">The input string to escape.</param>
    /// <returns>The escaped string, safe for use in SQL queries.</returns>
    public virtual string EscapeString(string val) => NPocoDatabaseExtensions.EscapeAtSymbols(val.Replace("'", "''"));

    /// <summary>
    /// Constructs a SQL expression that compares a string column to a parameter value for equality in a case-insensitive manner.
    /// Uses the SQL <c>UPPER</c> function to ensure the comparison ignores case, regardless of database collation settings.
    /// </summary>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramIndex">The index of the parameter to compare against (used to generate the parameter placeholder).</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns>A SQL string representing a case-insensitive equality comparison between the column and the parameter.</returns>
    public virtual string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType) =>
        //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
        $"upper({column}) = upper(@{paramIndex})";

    /// <summary>
    /// Generates a SQL expression for performing a case-insensitive wildcard (LIKE) comparison on a specified text column.
    /// Uses the SQL <c>UPPER</c> function to ensure case-insensitive matching regardless of database collation settings.
    /// </summary>
    /// <param name="column">The name of the column to compare.</param>
    /// <param name="paramIndex">The index of the parameter to use in the query (e.g., <c>@0</c>).</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns>A SQL string that compares the upper-cased column value to the upper-cased parameter using <c>LIKE</c> for wildcard matching.</returns>
    public virtual string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType) =>
        //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
        $"upper({column}) LIKE upper(@{paramIndex})";

    /// <summary>
    /// Generates a SQL expression that concatenates the specified arguments using the SQL CONCAT function.
    /// </summary>
    /// <param name="args">The strings or SQL expressions to concatenate.</param>
    /// <returns>A SQL string representing the CONCAT function with the provided arguments.</returns>
    public virtual string GetConcat(params string[] args) => "CONCAT(" + string.Join(",", args) + ")";

    /// <summary>
    /// Returns the table name wrapped in quotes.
    /// </summary>
    /// <param name="tableName">The name of the table to quote.</param>
    /// <returns>The quoted table name.</returns>
    public virtual string GetQuotedTableName(string? tableName) => $"\"{tableName}\"";

    /// <summary>
    /// Returns the column name wrapped in quotes for use in SQL queries.
    /// </summary>
    /// <param name="columnName">The name of the column to quote.</param>
    /// <returns>The quoted column name.</returns>
    public virtual string GetQuotedColumnName(string? columnName) => $"\"{columnName}\"";

    /// <summary>
    /// Generates an ORDER BY clause for a GUID column by converting its value to uppercase, ensuring case-insensitive ordering in SQL queries.
    /// </summary>
    /// <param name="tableName">The name of the table containing the GUID column.</param>
    /// <param name="columnName">The name of the GUID column to order by.</param>
    /// <returns>A SQL ORDER BY clause string for the specified GUID column.</returns>
    public virtual string OrderByGuid(string tableName, string columnName) => $"UPPER({this.GetQuotedColumn(tableName, columnName)})";

    /// <summary>
    /// Returns the specified name quoted with double quotes for use in SQL statements.
    /// </summary>
    /// <param name="name">The name to quote. Can be <c>null</c>.</param>
    /// <returns>The quoted name as a string, or <c>"null"</c> if <paramref name="name"/> is <c>null</c>.</returns
    public virtual string GetQuotedName(string? name) => $"\"{name}\"";

    /// <summary>
    /// Returns the specified string value enclosed in single quotes.
    /// </summary>
    /// <param name="value">The string value to quote.</param>
    /// <returns>The quoted string value.</returns>
    public virtual string GetQuotedValue(string value) => $"'{value}'";

    /// <inheritdoc />
    public virtual string GetNullCastSuffix<T>() => string.Empty;

    /// <summary>
    /// Returns the SQL index type string corresponding to the specified <see cref="IndexTypes"/> flags.
    /// </summary>
    /// <param name="indexTypes">The <see cref="IndexTypes"/> value indicating the type and uniqueness of the index.</param>
    /// <returns>A string representing the SQL index type, such as "UNIQUE CLUSTERED" or "NONCLUSTERED".</returns>
    public virtual string GetIndexType(IndexTypes indexTypes)
    {
        var indexType = string.Empty;

        if (indexTypes == IndexTypes.UniqueClustered || indexTypes == IndexTypes.UniqueNonClustered)
        {
            indexType += " UNIQUE";
        }

        if (indexTypes == IndexTypes.UniqueClustered || indexTypes == IndexTypes.Clustered)
        {
            indexType += " CLUSTERED";
        }
        else
        {
            indexType += " NONCLUSTERED";
        }

        return indexType.Trim();
    }

    /// <summary>
    /// Returns the SQL string representation for a given <see cref="SpecialDbType"/> value.
    /// </summary>
    /// <param name="dbType">The <see cref="SpecialDbType"/> for which to retrieve the SQL type string.</param>
    /// <returns>A <see cref="string"/> representing the SQL type corresponding to the specified <paramref name="dbType"/>.</returns>
    public virtual string GetSpecialDbType(SpecialDbType dbType)
    {
        if (dbType == SpecialDbType.NCHAR)
        {
            return SpecialDbType.NCHAR;
        }

        if (dbType == SpecialDbType.NTEXT)
        {
            return SpecialDbType.NTEXT;
        }

        if (dbType == SpecialDbType.NVARCHARMAX)
        {
            return "NVARCHAR(MAX)";
        }

        return "NVARCHAR";
    }

    /// <summary>
    /// Returns the fully qualified and quoted column name for the specified database type, including table name and optional aliasing.
    /// </summary>
    /// <param name="dbType">The type of the database.</param>
    /// <param name="tableName">The name of the table (will be quoted).</param>
    /// <param name="columnName">The name of the column (will be quoted).</param>
    /// <param name="columnAlias">An optional alias for the column. If specified, the result will include an <c>AS</c> clause.</param>
    /// <param name="referenceName">An optional prefix for the alias. If provided, it will be prepended to the alias, separated by double underscores.</param>
    /// <param name="forInsert">Indicates whether the column is for an insert operation. (Currently not used in formatting.)</param>
    /// <returns>A string containing the quoted table and column name, optionally followed by an <c>AS</c> clause with the quoted alias.</returns>
    public virtual string GetColumn(DatabaseType dbType, string tableName, string columnName, string? columnAlias, string? referenceName = null, bool forInsert = false)
    {
        tableName = GetQuotedTableName(tableName);
        columnName = GetQuotedColumnName(columnName);
        var column = tableName + "." + columnName;
        if (columnAlias == null)
        {
            return column;
        }

        referenceName = referenceName == null ? string.Empty : referenceName + "__";
        columnAlias = GetQuotedColumnName(referenceName + columnAlias);
        column += " AS " + columnAlias;
        return column;
    }


    /// <summary>
    /// Gets the default isolation level used for database transactions by this SQL syntax provider.
    /// </summary>
    public abstract IsolationLevel DefaultIsolationLevel { get; }

    /// <summary>
    /// Gets the name or identifier of the database provider used by this SQL syntax provider.
    /// </summary>
    public abstract string DbProvider { get; }

    /// <summary>
    /// Retrieves the names of all tables in the current database schema using the specified <paramref name="db"/> instance.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> instance to query for table names.</param>
    /// <returns>An <see cref="IEnumerable{string}"/> containing the names of tables in the schema.</returns>
    public virtual IEnumerable<string> GetTablesInSchema(IDatabase db) => new List<string>();

    /// <summary>
    /// Retrieves all columns defined in the database schema using the specified database instance.
    /// </summary>
    /// <param name="db">The database instance from which to retrieve schema column information.</param>
    /// <returns>An enumerable collection of <see cref="ColumnInfo"/> objects representing the columns in the schema.</returns>
    public virtual IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db) => new List<ColumnInfo>();

    /// <summary>
    /// Retrieves the constraints defined for each table in the specified database.
    /// </summary>
    /// <param name="db">The database instance from which to retrieve table constraints.</param>
    /// <returns>An enumerable of tuples, where each tuple contains the table name and the associated constraint name.</returns>
    public virtual IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db) =>
        new List<Tuple<string, string>>();

    /// <summary>
    /// Retrieves database constraint information for each column in the specified database.
    /// </summary>
    /// <param name="db">The database instance from which to retrieve column constraints.</param>
    /// <returns>
    /// An enumerable of tuples, each containing the table name, column name, and constraint name for a column.
    /// The base implementation returns an empty list.
    /// </returns>
    public virtual IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db) =>
        new List<Tuple<string, string, string>>();

    /// <summary>
    /// Retrieves all indexes defined in the specified database.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/> instance to query for index definitions.</param>
    /// <returns>
    /// An enumerable of tuples, where each tuple contains:
    /// <list type="bullet">
    ///   <item><description>The table name (<c>string</c>).</description></item>
    ///   <item><description>The index name (<c>string</c>).</description></item>
    ///   <item><description>The column name (<c>string</c>).</description></item>
    ///   <item><description>A <c>bool</c> indicating whether the index is unique.</description></item>
    /// </list>
    /// </returns>
    public abstract IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db);

    /// <summary>
    /// Attempts to get the default constraint name for a specified column in a table.
    /// </summary>
    /// <param name="db">The database instance to query.</param>
    /// <param name="tableName">The name of the table containing the column. Can be null.</param>
    /// <param name="columnName">The name of the column to find the default constraint for.</param>
    /// <param name="constraintName">When this method returns, contains the default constraint name if found; otherwise, null.</param>
    /// <returns>True if the default constraint was found; otherwise, false.</returns>
    public abstract bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName);

    /// <summary>
    /// Determines whether the specified primary key exists on the given table.
    /// </summary>
    /// <param name="db">The database instance to query.</param>
    /// <param name="tableName">The name of the table to check.</param>
    /// <param name="primaryKeyName">The name of the primary key to look for.</param>
    /// <returns>True if the primary key exists; otherwise, false.</returns>
    public virtual bool DoesPrimaryKeyExist(IDatabase db, string tableName, string primaryKeyName) => throw new NotImplementedException();

    /// <summary>
    /// Returns the formatted field name to be used in an SQL UPDATE statement, based on the specified field selector expression.
    /// </summary>
    /// <typeparam name="TDto">The type representing the data transfer object (DTO) containing the field.</typeparam>
    /// <param name="fieldSelector">An expression that selects the field to be updated.</param>
    /// <param name="tableAlias">An optional table alias to prefix the field name with, if required.</param>
    /// <returns>The formatted field name, optionally prefixed with the table alias, suitable for use in an UPDATE statement.</returns>
    public virtual string GetFieldNameForUpdate<TDto>(
        Expression<Func<TDto, object?>> fieldSelector,
        string? tableAlias = null) => this.GetFieldName(fieldSelector, tableAlias);

    /// <summary>
    /// Optionally adds an update hint to the provided insert SQL statement.
    /// </summary>
    /// <param name="sql">The SQL statement to which an update hint may be added.</param>
    /// <returns>The original or modified SQL statement, depending on the implementation.</returns>
    /// <remarks>
    /// The base implementation returns the SQL statement unchanged. Derived classes may override this method to add provider-specific update hints.
    /// </remarks>
    public virtual Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql) => sql;

    /// <summary>
    /// Optionally appends a SQL hint for update operations to the provided SQL query.
    /// </summary>
    /// <param name="sql">The SQL query to which an update hint may be appended.</param>
    /// <returns>The original or modified SQL query, depending on the implementation.</returns>
    public virtual Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql) => sql;

    /// <summary>
    /// Constructs a left join clause with a nested join on the specified SQL query, allowing for complex join scenarios.
    /// </summary>
    /// <typeparam name="TDto">The type representing the table or entity to join.</typeparam>
    /// <param name="sql">The base SQL query to which the left join and nested join will be applied.</param>
    /// <param name="nestedJoin">A function that defines the nested join logic to be included within the left join.</param>
    /// <param name="alias">An optional alias for the joined table.</param>
    /// <returns>A <see cref="SqlJoinClause{ISqlContext}"/> representing the constructed left join with the nested join applied.</returns>
    public abstract Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
        string? alias = null);

    /// <summary>
    /// Gets a dictionary that maps target types to their corresponding scalar mappers.
    /// The dictionary is keyed by the target <see cref="Type"/> and the value is an <see cref="IScalarMapper"/>.
    /// </summary>
    public virtual IDictionary<Type, IScalarMapper>? ScalarMappers => null;

    /// <summary>
    /// Checks if a table with the specified name exists in the database schema.
    /// </summary>
    /// <param name="db">The database instance to query.</param>
    /// <param name="tableName">The name of the table to check.</param>
    /// <returns><c>true</c> if the table exists; otherwise, <c>false</c>.</returns>
    public virtual bool DoesTableExist(IDatabase db, string tableName) => GetTablesInSchema(db).Contains(tableName);

    /// <summary>
    /// Indicates whether clustered indexes are supported by this SQL syntax provider.
    /// </summary>
    /// <returns><c>true</c> if clustered indexes are supported; otherwise, <c>false</c>.</returns>
    public virtual bool SupportsClustered() => true;

    /// <summary>
    /// Determines whether identity insert is supported by the SQL syntax provider.
    /// </summary>
    /// <returns>True if identity insert is supported; otherwise, false.</returns>
    public virtual bool SupportsIdentityInsert() => true;

    /// <inheritdoc />
    public virtual bool SupportsSequences() => false;

    /// <inheritdoc />
    public virtual void AlterSequences(IUmbracoDatabase database) => throw new NotSupportedException();

    /// <inheritdoc />
    public virtual void AlterSequences(IUmbracoDatabase database, string tableName) => throw new NotSupportedException();

    /// <summary>
    ///     This is used ONLY if we need to format datetime without using SQL parameters (i.e. during migrations)
    /// </summary>
    /// <param name="date">The date to format.</param>
    /// <param name="includeTime">Whether to include the time component.</param>
    /// <returns>The formatted date string.</returns>
    /// <remarks>
    ///     MSSQL has a DateTime standard that is unambiguous and works on all servers:
    ///     YYYYMMDD HH:mm:ss
    /// </remarks>
    public virtual string FormatDateTime(DateTime date, bool includeTime = true) =>
        // need CultureInfo.InvariantCulture because ":" here is the "time separator" and
        // may be converted to something else in different cultures (eg "." in DK).
        date.ToString(includeTime ? "yyyyMMdd HH:mm:ss" : "yyyyMMdd", CultureInfo.InvariantCulture);

    /// <summary>
    /// Formats a SQL create table statement for the specified table definition.
    /// </summary>
    /// <param name="table">The table definition to format.</param>
    /// <returns>A SQL string representing the create table statement.</returns>
    public virtual string Format(TableDefinition table)
    {
        var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), Format(table.Columns));

        return statement;
    }

    /// <summary>
    /// Formats the specified index definitions as a list of SQL statements.
    /// </summary>
    /// <param name="indexes">The index definitions to format.</param>
    /// <returns>A list of SQL strings representing the formatted index statements.</returns>
    public virtual List<string> Format(IEnumerable<IndexDefinition> indexes) => indexes.Select(Format).ToList();

    /// <summary>
    /// Formats a SQL create index statement for the specified <see cref="IndexDefinition"/>.
    /// </summary>
    /// <param name="index">The <see cref="IndexDefinition"/> to format.</param>
    /// <returns>A SQL string representing the create index statement.</returns>
    public virtual string Format(IndexDefinition index)
    {
        var name = string.IsNullOrEmpty(index.Name)
            ? $"IX_{index.TableName}_{index.ColumnName}"
            : index.Name;

        var columns = index.Columns.Any()
            ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
            : GetQuotedColumnName(index.ColumnName);

        return string.Format(
            CreateIndex,
            GetIndexType(index.IndexType),
            " ",
            GetQuotedName(name),
            GetQuotedTableName(index.TableName),
            columns);
    }

    /// <summary>
    /// Formats a collection of <see cref="ForeignKeyDefinition"/> objects into their corresponding SQL statements.
    /// </summary>
    /// <param name="foreignKeys">A collection of foreign key definitions to be formatted.</param>
    /// <returns>A list of SQL strings representing the formatted foreign key definitions.</returns>
    public virtual List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys) =>
        foreignKeys.Select(Format).ToList();

    /// <summary>
    /// Formats a SQL foreign key constraint statement for the specified <see cref="ForeignKeyDefinition"/>.
    /// </summary>
    /// <param name="foreignKey">The <see cref="ForeignKeyDefinition"/> to format.</param>
    /// <returns>A SQL string representing the foreign key constraint statement.</returns>
    public virtual string Format(ForeignKeyDefinition foreignKey)
    {
        var constraintName = string.IsNullOrEmpty(foreignKey.Name)
            ? $"FK_{foreignKey.ForeignTable}_{foreignKey.PrimaryTable}_{foreignKey.PrimaryColumns.First()}"
            : foreignKey.Name;

        return string.Format(
            CreateForeignKeyConstraint,
            GetQuotedTableName(foreignKey.ForeignTable),
            GetQuotedName(constraintName),
            GetQuotedColumnName(foreignKey.ForeignColumns.First()),
            GetQuotedTableName(foreignKey.PrimaryTable),
            GetQuotedColumnName(foreignKey.PrimaryColumns.First()),
            FormatCascade("DELETE", foreignKey.OnDelete),
            FormatCascade("UPDATE", foreignKey.OnUpdate));
    }

    /// <summary>
    /// Formats the specified collection of column definitions into a SQL string.
    /// </summary>
    /// <param name="columns">The collection of column definitions to format.</param>
    /// <returns>A SQL string representing the formatted columns.</returns>
    public virtual string Format(IEnumerable<ColumnDefinition> columns)
    {
        var sb = new StringBuilder();
        foreach (ColumnDefinition column in columns)
        {
            sb.Append(Format(column) + ",\n");
        }

        return sb.ToString().TrimEnd(",\n");
    }

    /// <summary>
    /// Converts the specified <see cref="ColumnDefinition"/> into its SQL string representation.
    /// </summary>
    /// <param name="column">The <see cref="ColumnDefinition"/> to format as SQL.</param>
    /// <returns>A string containing the SQL representation of the column definition.</returns>
    public virtual string Format(ColumnDefinition column) =>
        string.Join(" ", ClauseOrder
            .Select(action => action(column))
            .Where(clause => string.IsNullOrEmpty(clause) == false));

    /// <summary>
    /// Formats a <see cref="ColumnDefinition"/> for a specific table into a SQL string suitable for use in a CREATE or ALTER TABLE statement.
    /// </summary>
    /// <param name="column">The column definition to format.</param>
    /// <param name="tableName">The name of the table to which the column belongs.</param>
    /// <param name="sqls">When this method returns, contains any additional SQL statements required for the column definition (such as constraints or indexes), if applicable.</param>
    /// <returns>A SQL string representing the formatted column definition.</returns>
    public virtual string Format(ColumnDefinition column, string tableName, out IEnumerable<string> sqls)
    {
        var sql = new StringBuilder();
        sql.Append(FormatString(column));
        sql.Append(" ");
        sql.Append(FormatType(column));
        sql.Append(" ");
        sql.Append("NULL"); // always nullable
        sql.Append(" ");
        sql.Append(FormatConstraint(column));
        sql.Append(" ");
        sql.Append(FormatDefaultValue(column));
        sql.Append(" ");
        sql.Append(FormatPrimaryKey(column));
        sql.Append(" ");
        sql.Append(FormatIdentity(column));

        //var isNullable = column.IsNullable;

        //var constraint = FormatConstraint(column)?.TrimStart("CONSTRAINT ");
        //var hasConstraint = !string.IsNullOrWhiteSpace(constraint);

        //var defaultValue = FormatDefaultValue(column);
        //var hasDefaultValue = !string.IsNullOrWhiteSpace(defaultValue);

        // TODO: This used to exit if nullable but that means this would never work
        // to return SQL if the column was nullable?!? I don't get it. This was here
        // 4 years ago, I've removed it so that this works for nullable columns.
        //if (isNullable /*&& !hasConstraint && !hasDefaultValue*/)
        //{
        //    sqls = Enumerable.Empty<string>();
        //    return sql.ToString();
        //}

        var msql = new List<string>();
        sqls = msql;

        var alterSql = new StringBuilder();
        alterSql.Append(FormatString(column));
        alterSql.Append(" ");
        alterSql.Append(FormatType(column));
        alterSql.Append(" ");
        alterSql.Append(FormatNullable(column));
        //alterSql.Append(" ");
        //alterSql.Append(FormatPrimaryKey(column));
        //alterSql.Append(" ");
        //alterSql.Append(FormatIdentity(column));
        msql.Add(string.Format(AlterColumn, tableName, alterSql));

        //if (hasConstraint)
        //{
        //    var dropConstraintSql = string.Format(DeleteConstraint, tableName, constraint);
        //    msql.Add(dropConstraintSql);
        //    var constraintType = hasDefaultValue ? defaultValue : "";
        //    var createConstraintSql = string.Format(CreateConstraint, tableName, constraint, constraintType, FormatString(column));
        //    msql.Add(createConstraintSql);
        //}

        return sql.ToString();
    }

    /// <summary>
    /// Generates the SQL primary key constraint definition for the specified table.
    /// </summary>
    /// <param name="table">The <see cref="TableDefinition"/> containing column and primary key information.</param>
    /// <returns>
    /// A string representing the SQL primary key constraint for the table, or an empty string if no primary key is defined.
    /// </returns>
    public virtual string FormatPrimaryKey(TableDefinition table)
    {
        ColumnDefinition? columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
        if (columnDefinition == null)
        {
            return string.Empty;
        }

        var constraintName = string.IsNullOrEmpty(columnDefinition.PrimaryKeyName)
            ? $"PK_{table.Name}"
            : columnDefinition.PrimaryKeyName;

        var columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
            ? GetQuotedColumnName(columnDefinition.Name)
            : string.Join(", ", columnDefinition.PrimaryKeyColumns
                .Split(Constants.CharArrays.CommaSpace, StringSplitOptions.RemoveEmptyEntries)
                .Select(GetQuotedColumnName));

        var primaryKeyPart =
            string.Concat("PRIMARY KEY", columnDefinition.IsIndexed ? " CLUSTERED" : " NONCLUSTERED");

        return string.Format(
            CreateConstraint,
            GetQuotedTableName(table.Name),
            GetQuotedName(constraintName),
            primaryKeyPart,
            columns);
    }

    /// <summary>
    /// Formats the SQL statement to rename a column in a table.
    /// </summary>
    /// <param name="tableName">The name of the table containing the column to rename.</param>
    /// <param name="oldName">The current name of the column.</param>
    /// <param name="newName">The new name for the column.</param>
    /// <returns>A SQL string that renames the specified column.</returns>
    public virtual string FormatColumnRename(string? tableName, string? oldName, string? newName) =>
        string.Format(
            RenameColumn,
            GetQuotedTableName(tableName),
            GetQuotedColumnName(oldName),
            GetQuotedColumnName(newName));

    /// <summary>
    /// Formats the SQL statement to rename a table from <paramref name="oldName"/> to <paramref name="newName"/>.
    /// </summary>
    /// <param name="oldName">The current name of the table to rename.</param>
    /// <param name="newName">The new name for the table.</param>
    /// <returns>The formatted SQL rename table statement.</returns>
    public virtual string FormatTableRename(string? oldName, string? newName) =>
        string.Format(RenameTable, GetQuotedTableName(oldName), GetQuotedTableName(newName));

    /// <summary>
    /// Constructs a fully qualified and quoted column name, optionally including an alias.
    /// </summary>
    /// <param name="tableNameOrAlias">The table name or alias to prefix to the column name.</param>
    /// <param name="columnName">The name of the column to be quoted.</param>
    /// <param name="columnAlias">An optional alias for the column; if specified, it will be quoted and appended using <c>AS</c>.</param>
    /// <returns>A string containing the prefixed and quoted column name, with an optional quoted alias.</returns>
    public virtual string ColumnWithAlias(string tableNameOrAlias, string columnName, string columnAlias = "")
    {
        var quotedColumnName = GetQuotedColumnName(columnName);
        var columnPrefix = GetColumnPrefix(tableNameOrAlias);
        var asAppendix = string.IsNullOrEmpty(columnAlias)
            ? string.Empty
            : $" AS {GetQuotedName(columnAlias)}";

        return $"{columnPrefix}{quotedColumnName}{asAppendix}";
    }

    private string GetColumnPrefix(string? tableNameOrAlias)
    {
        if (string.IsNullOrEmpty(tableNameOrAlias))
        {
            return string.Empty;
        }

        // Always quote the identifier to avoid ambiguity between table names and aliases.
        var quoted = GetQuotedTableName(tableNameOrAlias.Trim());
        return $"{quoted}.";
    }

    /// <summary>
    /// Modifies the specified SQL query to return only the specified number of top rows.
    /// </summary>
    /// <param name="sql">The SQL query to be modified.</param>
    /// <param name="top">The maximum number of rows to return.</param>
    /// <returns>The modified SQL query that selects only the top rows.</returns>
    public abstract Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top);

    /// <summary>
    /// Creates a table in the specified database using the provided table definition.
    /// </summary>
    /// <param name="database">The database in which to create the table.</param>
    /// <param name="tableDefinition">The definition of the table to be created.</param>
    /// <param name="skipKeysAndIndexes">If true, skips the creation of keys and indexes for the table.</param>
    public abstract void HandleCreateTable(
        IDatabase database,
        TableDefinition tableDefinition,
        bool skipKeysAndIndexes = false);

    /// <summary>
    /// Gets the SQL command to delete a default constraint.
    /// By default, this property throws a <see cref="NotSupportedException"/> indicating that default constraints are not supported.
    /// </summary>
    public virtual string DeleteDefaultConstraint =>
        throw new NotSupportedException("Default constraints are not supported");

    /// <summary>
    /// Gets the SQL syntax keyword used to calculate the length of a string in SQL queries.
    /// Returns the string <c>LEN</c>, which is the standard function for string length in SQL Server.
    /// </summary>
    public virtual string Length => "LEN";

    /// <summary>
    /// Gets the SQL keyword used for the substring function in SQL statements.
    /// </summary>
    public virtual string Substring => "SUBSTRING";

    /// <summary>
    /// Gets the SQL statement format string for creating a table, with placeholders for the table name and column definitions.
    /// </summary>
    public virtual string CreateTable => "CREATE TABLE {0} ({1})";

    /// <summary>
    /// Gets the SQL command format string for dropping a table, where <c>{0}</c> is replaced with the table name.
    /// </summary>
    public virtual string DropTable => "DROP TABLE {0}";

    /// <summary>
    /// Gets the SQL syntax format string for adding a column to a table.
    /// The format string expects the table name and column definition as parameters.
    /// </summary>
    public virtual string AddColumn => "ALTER TABLE {0} ADD {1}";

    /// <summary>
    /// Gets the SQL syntax template for dropping a column from a table.
    /// The placeholders {0} and {1} represent the table name and column name, respectively.
    /// </summary>
    public virtual string DropColumn => "ALTER TABLE {0} DROP COLUMN {1}";

    /// <summary>
    /// Gets the SQL syntax template for altering a column in a table.
    /// The returned string contains placeholders: {0} for the table name and {1} for the column definition.
    /// </summary>
    public virtual string AlterColumn => "ALTER TABLE {0} ALTER COLUMN {1}";

    /// <summary>
    /// Gets the SQL syntax template for renaming a column in a table.
    /// The template uses placeholders: {0} for the table name, {1} for the current column name, and {2} for the new column name.
    /// </summary>
    public virtual string RenameColumn => "ALTER TABLE {0} RENAME COLUMN {1} TO {2}";

    /// <summary>
    /// Gets the SQL syntax template for renaming a table, where <c>{0}</c> is the current table name and <c>{1}</c> is the new table name.
    /// </summary>
    public virtual string RenameTable => "RENAME TABLE {0} TO {1}";

    /// <summary>
    /// Gets the SQL statement used to create a schema, with a format placeholder for the schema name.
    /// The returned string is typically formatted with the desired schema name as an argument.
    /// </summary>
    public virtual string CreateSchema => "CREATE SCHEMA {0}";

    /// <summary>
    /// Gets the SQL command template for altering the schema of a database object.
    /// The template uses placeholders: {0} for the target schema, {1} for the current schema, and {2} for the object name.
    /// </summary>
    public virtual string AlterSchema => "ALTER SCHEMA {0} TRANSFER {1}.{2}";

    /// <summary>
    /// Gets the SQL command format string used to drop a schema, where <c>{0}</c> is replaced with the schema name.
    /// </summary>
    public virtual string DropSchema => "DROP SCHEMA {0}";

    /// <summary>
    /// Gets the SQL template string used to create an index.
    /// The template includes placeholders for index options and names: {0} for unique, {1} for clustered, {2} for index name, {3} for table name, and {4} for column list.
    /// </summary>
    public virtual string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3} ({4})";

    /// <summary>
    /// Gets the SQL statement template for dropping an index.
    /// </summary>
    public virtual string DropIndex => "DROP INDEX {0}";

    /// <summary>
    /// Gets the SQL insert statement format string, where
    /// {0} is the table name, {1} is the comma-separated list of column names, and {2} is the comma-separated list of values.
    /// </summary>
    public virtual string InsertData => "INSERT INTO {0} ({1}) VALUES ({2})";

    /// <summary>
    /// Gets the SQL syntax format string for an update statement, with placeholders for the table name, set clause, and where clause.
    /// Format: "UPDATE {0} SET {1} WHERE {2}".
    /// </summary>
    public virtual string UpdateData => "UPDATE {0} SET {1} WHERE {2}";

    /// <summary>
    /// Gets a format string for a SQL DELETE statement, with placeholders for the table name and the WHERE condition.
    /// </summary>
    public virtual string DeleteData => "DELETE FROM {0} WHERE {1}";

    /// <summary>
    /// Gets the SQL syntax template for truncating a table, with a placeholder for the table name.
    /// </summary>
    public virtual string TruncateTable => "TRUNCATE TABLE {0}";

    /// <summary>
    /// Gets the SQL statement template for creating a constraint on a table.
    /// The template uses placeholders:
    /// {0} - table name, {1} - constraint name, {2} - constraint type (e.g., PRIMARY KEY), {3} - column list.
    /// </summary>
    public virtual string CreateConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})";

    /// <summary>
    /// Gets the SQL format string used to delete a constraint from a table.
    /// The format string expects the table name and constraint name as arguments.
    /// </summary>
    public virtual string DeleteConstraint => "ALTER TABLE {0} DROP CONSTRAINT {1}";

    /// <summary>
    /// Gets the SQL expression that converts an integer to a zero-padded, orderable string representation.
    /// This is typically used to ensure that integer values are sorted correctly as strings in SQL queries.
    /// </summary>
    public virtual string ConvertIntegerToOrderableString => "REPLACE(STR({0}, 8), SPACE(1), '0')";

    /// <summary>
    /// Gets the SQL expression that converts a date value to an ISO 8601 string (format 120),
    /// which is suitable for lexicographical ordering in SQL queries.
    /// The expression uses a format string where the date column or value should be substituted for {0}.
    /// </summary>
    public virtual string ConvertDateToOrderableString => "CONVERT(nvarchar, {0}, 120)";

    /// <summary>
    /// Gets the SQL expression used to convert a decimal value into a string format that preserves numeric ordering when sorted lexicographically.
    /// This is typically used to ensure that decimal values are correctly ordered in string-based SQL operations.
    /// </summary>
    public virtual string ConvertDecimalToOrderableString => "REPLACE(STR({0}, 20, 9), SPACE(1), '0')";

    /// <summary>
    /// Gets the SQL expression used to convert a uniqueidentifier value to its string representation (nvarchar(36)).
    /// Typically used for formatting SQL queries that require uniqueidentifier-to-string conversion.
    /// </summary>
    public virtual string ConvertUniqueIdentifierToString => "CONVERT(nvarchar(36), {0})";

    /// <summary>
    /// Converts an integer value to its boolean representation as a string: "1" for non-zero values, "0" for zero.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>"1" if <paramref name="value"/> is non-zero; otherwise, "0".</returns>
    public virtual string ConvertIntegerToBoolean(int value) => value == 0 ? "0" : "1";

    private DbTypes InitColumnTypeMap()
    {
        var dbTypeMap = new DbTypesFactory();
        dbTypeMap.Set<string>(DbType.String, StringColumnDefinition);
        dbTypeMap.Set<char>(DbType.StringFixedLength, StringColumnDefinition);
        dbTypeMap.Set<char?>(DbType.StringFixedLength, StringColumnDefinition);
        dbTypeMap.Set<char[]>(DbType.String, StringColumnDefinition);
        dbTypeMap.Set<bool>(DbType.Boolean, BoolColumnDefinition);
        dbTypeMap.Set<bool?>(DbType.Boolean, BoolColumnDefinition);
        dbTypeMap.Set<Guid>(DbType.Guid, GuidColumnDefinition);
        dbTypeMap.Set<Guid?>(DbType.Guid, GuidColumnDefinition);
        dbTypeMap.Set<DateTime>(DbType.DateTime, DateTimeColumnDefinition);
        dbTypeMap.Set<DateTime?>(DbType.DateTime, DateTimeColumnDefinition);
        dbTypeMap.Set<TimeSpan>(DbType.Time, TimeColumnDefinition);
        dbTypeMap.Set<TimeSpan?>(DbType.Time, TimeColumnDefinition);
        dbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);
        dbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);
        dbTypeMap.Set<DateOnly>(DbType.Date, DateOnlyColumnDefinition);
        dbTypeMap.Set<DateOnly?>(DbType.Date, DateOnlyColumnDefinition);
        dbTypeMap.Set<TimeOnly>(DbType.Time, TimeOnlyColumnDefinition);
        dbTypeMap.Set<TimeOnly?>(DbType.Time, TimeOnlyColumnDefinition);

        dbTypeMap.Set<byte>(DbType.Byte, IntColumnDefinition);
        dbTypeMap.Set<byte?>(DbType.Byte, IntColumnDefinition);
        dbTypeMap.Set<sbyte>(DbType.SByte, IntColumnDefinition);
        dbTypeMap.Set<sbyte?>(DbType.SByte, IntColumnDefinition);
        dbTypeMap.Set<short>(DbType.Int16, IntColumnDefinition);
        dbTypeMap.Set<short?>(DbType.Int16, IntColumnDefinition);
        dbTypeMap.Set<ushort>(DbType.UInt16, IntColumnDefinition);
        dbTypeMap.Set<ushort?>(DbType.UInt16, IntColumnDefinition);
        dbTypeMap.Set<int>(DbType.Int32, IntColumnDefinition);
        dbTypeMap.Set<int?>(DbType.Int32, IntColumnDefinition);
        dbTypeMap.Set<uint>(DbType.UInt32, IntColumnDefinition);
        dbTypeMap.Set<uint?>(DbType.UInt32, IntColumnDefinition);

        dbTypeMap.Set<long>(DbType.Int64, LongColumnDefinition);
        dbTypeMap.Set<long?>(DbType.Int64, LongColumnDefinition);
        dbTypeMap.Set<ulong>(DbType.UInt64, LongColumnDefinition);
        dbTypeMap.Set<ulong?>(DbType.UInt64, LongColumnDefinition);

        dbTypeMap.Set<float>(DbType.Single, RealColumnDefinition);
        dbTypeMap.Set<float?>(DbType.Single, RealColumnDefinition);
        dbTypeMap.Set<double>(DbType.Double, RealColumnDefinition);
        dbTypeMap.Set<double?>(DbType.Double, RealColumnDefinition);

        dbTypeMap.Set<decimal>(DbType.Decimal, DecimalColumnDefinition);
        dbTypeMap.Set<decimal?>(DbType.Decimal, DecimalColumnDefinition);

        dbTypeMap.Set<byte[]>(DbType.Binary, BlobColumnDefinition);

        return dbTypeMap.Create();
    }

    /// <summary>
    /// Returns the SQL type string representation for a given <see cref="SpecialDbType"/> value.
    /// </summary>
    /// <param name="dbType">The special database type for which to retrieve the SQL type string.</param>
    /// <returns>The SQL type string corresponding to the specified <paramref name="dbType"/>.</returns>
    public virtual string GetSpecialDbType(SpecialDbType dbType, int customSize) =>
        $"{GetSpecialDbType(dbType)}({customSize})";

    protected virtual string FormatCascade(string onWhat, Rule rule)
    {
        var action = "NO ACTION";
        switch (rule)
        {
            case Rule.None:
                return string.Empty;
            case Rule.Cascade:
                action = "CASCADE";
                break;
            case Rule.SetNull:
                action = "SET NULL";
                break;
            case Rule.SetDefault:
                action = "SET DEFAULT";
                break;
        }

        return $" ON {onWhat} {action}";
    }

    protected virtual string FormatString(ColumnDefinition column) => GetQuotedColumnName(column.Name);

    protected virtual string FormatType(ColumnDefinition column)
    {
        if (column.Type.HasValue == false && string.IsNullOrEmpty(column.CustomType) == false)
        {
            return column.CustomType;
        }

        if (column.CustomDbType.HasValue)
        {
            if (column.Size != default)
            {
                return GetSpecialDbType(column.CustomDbType.Value, column.Size);
            }

            return GetSpecialDbType(column.CustomDbType.Value);
        }

        Type type = column.Type.HasValue
            ? DbTypeMap.ColumnDbTypeMap.First(x => x.Value == column.Type.Value).Key
            : column.PropertyType;

        if (type == typeof(string))
        {
            var valueOrDefault = column.Size != default ? column.Size : DefaultStringLength;
            return string.Format(StringLengthColumnDefinitionFormat, valueOrDefault);
        }

        if (type == typeof(decimal))
        {
            var precision = column.Size != default ? column.Size : DefaultDecimalPrecision;
            var scale = column.Precision != default ? column.Precision : DefaultDecimalScale;
            return string.Format(DecimalColumnDefinitionFormat, precision, scale);
        }

        var definition = DbTypeMap.ColumnTypeMap[type];
        var dbTypeDefinition = column.Size != default
            ? $"{definition}({column.Size})"
            : definition;
        //NOTE Precision is left out
        return dbTypeDefinition;
    }

    protected virtual string FormatNullable(ColumnDefinition column) => column.IsNullable ? "NULL" : "NOT NULL";

    protected virtual string FormatConstraint(ColumnDefinition column)
    {
        if (string.IsNullOrEmpty(column.ConstraintName) && column.DefaultValue == null)
        {
            return string.Empty;
        }

        return
            $"CONSTRAINT {(string.IsNullOrEmpty(column.ConstraintName) ? GetQuotedName($"DF_{column.TableName}_{column.Name}") : column.ConstraintName)}";
    }

    protected virtual string FormatDefaultValue(ColumnDefinition column)
    {
        if (column.DefaultValue == null)
        {
            return string.Empty;
        }

        // HACK: probably not needed with latest changes
        if (string.Equals(column.DefaultValue.ToString(), "GETDATE()", StringComparison.OrdinalIgnoreCase))
        {
            column.DefaultValue = SystemMethods.CurrentDateTime;
        }
        else if (string.Equals(column.DefaultValue.ToString(), "GETUTCDATE()", StringComparison.OrdinalIgnoreCase))
        {
            column.DefaultValue = SystemMethods.CurrentUTCDateTime;
        }

        // see if this is for a system method
        if (column.DefaultValue is SystemMethods)
        {
            var method = FormatSystemMethods((SystemMethods)column.DefaultValue);
            return string.IsNullOrEmpty(method) ? string.Empty : string.Format(DefaultValueFormat, method);
        }

        return string.Format(DefaultValueFormat, GetQuotedValue(column.DefaultValue.ToString()!));
    }

    protected virtual string FormatPrimaryKey(ColumnDefinition column) => string.Empty;

    protected abstract string? FormatSystemMethods(SystemMethods systemMethod);

    protected abstract string FormatIdentity(ColumnDefinition column);

    /// <inheritdoc />
    public virtual string TruncateConstraintName<T>(string constraintName) => constraintName;
}
