using System.Reflection;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public static class DefinitionFactory
{
    public static TableDefinition GetTableDefinition(Type modelType, ISqlSyntaxProvider sqlSyntax)
    {
        // Looks for NPoco's TableNameAtribute for the name of the table
        // If no attribute is set we use the name of the Type as the default convention
        TableNameAttribute? tableNameAttribute = modelType.FirstAttribute<TableNameAttribute>();
        var tableName = tableNameAttribute == null ? modelType.Name : tableNameAttribute.Value;

        var tableDefinition = new TableDefinition { Name = tableName };
        var objProperties = modelType.GetProperties().ToList();
        foreach (PropertyInfo propertyInfo in objProperties)
        {
            // If current property has an IgnoreAttribute then skip it
            IgnoreAttribute? ignoreAttribute = propertyInfo.FirstAttribute<IgnoreAttribute>();
            if (ignoreAttribute != null)
            {
                continue;
            }

            // If current property has a ResultColumnAttribute then skip it
            ResultColumnAttribute? resultColumnAttribute = propertyInfo.FirstAttribute<ResultColumnAttribute>();
            if (resultColumnAttribute != null)
            {
                continue;
            }

            // Looks for ColumnAttribute with the name of the column, which would exist with ExplicitColumns
            // Otherwise use the name of the property itself as the default convention
            ColumnAttribute? columnAttribute = propertyInfo.FirstAttribute<ColumnAttribute>();
            var columnName = columnAttribute != null ? columnAttribute.Name : propertyInfo.Name;
            ColumnDefinition columnDefinition =
                GetColumnDefinition(modelType, propertyInfo, columnName, tableName, sqlSyntax);
            tableDefinition.Columns.Add(columnDefinition);

            // Creates a foreignkey definition and adds it to the collection on the table definition
            IEnumerable<ForeignKeyAttribute>? foreignKeyAttributes =
                propertyInfo.MultipleAttribute<ForeignKeyAttribute>();
            if (foreignKeyAttributes != null)
            {
                foreach (ForeignKeyAttribute foreignKeyAttribute in foreignKeyAttributes)
                {
                    ForeignKeyDefinition foreignKeyDefinition = GetForeignKeyDefinition(modelType, propertyInfo, foreignKeyAttribute, columnName, tableName);
                    tableDefinition.ForeignKeys.Add(foreignKeyDefinition);
                }
            }

            // Creates an index definition and adds it to the collection on the table definition
            IndexAttribute? indexAttribute = propertyInfo.FirstAttribute<IndexAttribute>();
            if (indexAttribute != null)
            {
                IndexDefinition indexDefinition =
                    GetIndexDefinition(modelType, propertyInfo, indexAttribute, columnName, tableName);
                tableDefinition.Indexes.Add(indexDefinition);
            }
        }

        return tableDefinition;
    }

    public static ColumnDefinition GetColumnDefinition(Type modelType, PropertyInfo propertyInfo, string columnName, string tableName, ISqlSyntaxProvider sqlSyntax)
    {
        var definition = new ColumnDefinition
        {
            Name = columnName,
            TableName = tableName,
            ModificationType = ModificationType.Create,
        };

        // Look for specific Null setting attributed a column
        NullSettingAttribute? nullSettingAttribute = propertyInfo.FirstAttribute<NullSettingAttribute>();
        if (nullSettingAttribute != null)
        {
            definition.IsNullable = nullSettingAttribute.NullSetting == NullSettings.Null;
        }

        // Look for specific DbType attributed a column
        SpecialDbTypeAttribute? databaseTypeAttribute = propertyInfo.FirstAttribute<SpecialDbTypeAttribute>();
        if (databaseTypeAttribute != null)
        {
            definition.CustomDbType = databaseTypeAttribute.DatabaseType;
        }
        else
        {
            definition.PropertyType = propertyInfo.PropertyType;
        }

        // Look for Primary Key for the current column
        PrimaryKeyColumnAttribute? primaryKeyColumnAttribute = propertyInfo.FirstAttribute<PrimaryKeyColumnAttribute>();
        if (primaryKeyColumnAttribute != null)
        {
            var primaryKeyName = string.IsNullOrEmpty(primaryKeyColumnAttribute.Name)
                ? string.Format("PK_{0}", tableName)
                : primaryKeyColumnAttribute.Name;

            definition.IsPrimaryKey = true;
            definition.IsIdentity = primaryKeyColumnAttribute.AutoIncrement;
            definition.IsIndexed = primaryKeyColumnAttribute.Clustered;
            definition.PrimaryKeyName = primaryKeyName;
            definition.PrimaryKeyColumns = primaryKeyColumnAttribute.OnColumns ?? string.Empty;
            definition.Seeding = primaryKeyColumnAttribute.IdentitySeed;
        }

        // Look for Size/Length of DbType
        LengthAttribute? lengthAttribute = propertyInfo.FirstAttribute<LengthAttribute>();
        if (lengthAttribute != null)
        {
            definition.Size = lengthAttribute.Length;
        }

        // Look for Constraint for the current column
        ConstraintAttribute? constraintAttribute = propertyInfo.FirstAttribute<ConstraintAttribute>();
        if (constraintAttribute != null)
        {
            definition.ConstraintName = constraintAttribute.Name ?? string.Empty;
            definition.DefaultValue = constraintAttribute.Default ?? string.Empty;
        }

        return definition;
    }

    public static ForeignKeyDefinition GetForeignKeyDefinition(Type modelType, PropertyInfo propertyInfo, ForeignKeyAttribute attribute, string columnName, string tableName)
    {
        TableNameAttribute? referencedTable = attribute.Type.FirstAttribute<TableNameAttribute>();
        PrimaryKeyAttribute? referencedPrimaryKey = attribute.Type.FirstAttribute<PrimaryKeyAttribute>();

        var referencedColumn = string.IsNullOrEmpty(attribute.Column)
            ? referencedPrimaryKey!.Value
            : attribute.Column;

        var foreignKeyName = string.IsNullOrEmpty(attribute.Name)
            ? string.Format("FK_{0}_{1}_{2}", tableName, referencedTable!.Value, referencedColumn)
            : attribute.Name;

        var definition = new ForeignKeyDefinition
        {
            Name = foreignKeyName,
            ForeignTable = tableName,
            PrimaryTable = referencedTable!.Value,
            OnDelete = attribute.OnDelete,
            OnUpdate = attribute.OnUpdate,
        };
        definition.ForeignColumns.Add(columnName);
        definition.PrimaryColumns.Add(referencedColumn);

        return definition;
    }

    public static IndexDefinition GetIndexDefinition(Type modelType, PropertyInfo propertyInfo, IndexAttribute attribute, string columnName, string tableName)
    {
        var indexName = string.IsNullOrEmpty(attribute.Name)
            ? string.Format("IX_{0}_{1}", tableName, columnName)
            : attribute.Name;

        var definition = new IndexDefinition
        {
            Name = indexName,
            IndexType = attribute.IndexType,
            ColumnName = columnName,
            TableName = tableName,
        };

        if (string.IsNullOrEmpty(attribute.ForColumns) == false)
        {
            IEnumerable<string> columns = attribute.ForColumns.Split(Constants.CharArrays.Comma).Select(p => p.Trim());
            foreach (var column in columns)
            {
                definition.Columns.Add(new IndexColumnDefinition { Name = column, Direction = Direction.Ascending });
            }
        }

        if (string.IsNullOrEmpty(attribute.IncludeColumns) == false)
        {
            IEnumerable<string> columns = attribute.IncludeColumns.Split(',').Select(p => p.Trim());
            foreach (var column in columns)
            {
                definition.IncludeColumns.Add(
                    new IndexColumnDefinition { Name = column, Direction = Direction.Ascending });
            }
        }

        return definition;
    }
}
