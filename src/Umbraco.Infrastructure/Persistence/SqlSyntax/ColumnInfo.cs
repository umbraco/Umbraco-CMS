namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
/// Contains metadata and details about a column in a database table.
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo"/> class.
    /// </summary>
    /// <param name="tableName">The name of the table the column belongs to.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="ordinal">The ordinal position of the column in the table.</param>
    /// <param name="columnDefault">The default value of the column.</param>
    /// <param name="isNullable">Indicates whether the column allows null values.</param>
    /// <param name="dataType">The data type of the column.</param>
    public ColumnInfo(string tableName, string columnName, int ordinal, string columnDefault, string isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        ColumnDefault = columnDefault;
        IsNullable = isNullable?.Equals("YES") ?? false;
        DataType = dataType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo"/> class.
    /// </summary>
    /// <param name="tableName">The name of the table that contains the column.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="ordinal">The zero-based ordinal position of the column within the table.</param>
    /// <param name="isNullable">A string indicating whether the column allows null values (e.g., "YES" or "NO").</param>
    /// <param name="dataType">The data type of the column as defined in the database.</param>
    public ColumnInfo(string tableName, string columnName, int ordinal, string isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        IsNullable = isNullable?.Equals("YES") ?? false;
        DataType = dataType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo"/> class with the specified table name, column name, ordinal position, nullability, and data type.
    /// </summary>
    /// <param name="tableName">The name of the table to which the column belongs.</param>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="ordinal">The zero-based ordinal position of the column within the table.</param>
    /// <param name="isNullable">A value indicating whether the column allows null values.</param>
    /// <param name="dataType">The data type of the column.</param>
    public ColumnInfo(string tableName, string columnName, int ordinal, bool isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        IsNullable = isNullable;
        DataType = dataType;
    }

    /// <summary>
    /// Gets or sets the name of the table associated with the column.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the ordinal position of the column.
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Gets or sets the default value for the column.
    /// </summary>

    public string? ColumnDefault { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column allows null values.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the database data type of the column.
    /// </summary>
    public string DataType { get; set; }
}
