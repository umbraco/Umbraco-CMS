namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

public class ColumnInfo
{
    public ColumnInfo(string tableName, string columnName, int ordinal, string columnDefault, string isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        ColumnDefault = columnDefault;
        IsNullable = isNullable.Equals("YES");
        DataType = dataType;
    }

    public ColumnInfo(string tableName, string columnName, int ordinal, string isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        IsNullable = isNullable.Equals("YES");
        DataType = dataType;
    }

    public ColumnInfo(string tableName, string columnName, int ordinal, bool isNullable, string dataType)
    {
        TableName = tableName;
        ColumnName = columnName;
        Ordinal = ordinal;
        IsNullable = isNullable;
        DataType = dataType;
    }

    public string TableName { get; set; }

    public string ColumnName { get; set; }

    public int Ordinal { get; set; }

    public string? ColumnDefault { get; set; }

    public bool IsNullable { get; set; }

    public string DataType { get; set; }
}
