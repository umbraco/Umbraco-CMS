using System.Reflection;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Factory for creating database model definitions used in Umbraco CMS persistence.
/// </summary>
public static class DefinitionFactory
{
    /// <summary>
    /// Generates a <see cref="TableDefinition"/> for the specified model type, using the provided SQL syntax provider.
    /// The method inspects the model type's properties and associated attributes (such as <see cref="TableNameAttribute"/>,
    /// <see cref="ColumnAttribute"/>, <see cref="ForeignKeyAttribute"/>, <see cref="IndexAttribute"/>, <see cref="IgnoreAttribute"/>,
    /// and <see cref="ResultColumnAttribute"/>) to determine the table name, columns, foreign keys, and indexes.
    /// </summary>
    /// <param name="modelType">The model <see cref="Type"/> to generate the table definition for.</param>
    /// <param name="sqlSyntax">The SQL syntax provider used to generate database-specific SQL.</param>
    /// <returns>A <see cref="TableDefinition"/> representing the database table structure, including columns, foreign keys, and indexes, for the specified model type.</returns>
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
            IEnumerable<IndexAttribute>? indexAttributes = propertyInfo.MultipleAttribute<IndexAttribute>();

            if (indexAttributes == null)
            {
                continue;
            }

            foreach (IndexAttribute indexAttribute in indexAttributes)
            {
                IndexDefinition indexDefinition =
                    GetIndexDefinition(modelType, propertyInfo, indexAttribute, columnName, tableName);
                tableDefinition.Indexes.Add(indexDefinition);
            }
        }

        return tableDefinition;
    }

    /// <summary>
    /// Creates a <see cref="ColumnDefinition"/> for the specified property of a model type by inspecting its attributes and metadata.
    /// </summary>
    /// <param name="modelType">The type of the model containing the property.</param>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> representing the property to map as a database column.</param>
    /// <param name="columnName">The name of the column in the database.</param>
    /// <param name="tableName">The name of the table the column belongs to.</param>
    /// <param name="sqlSyntax">The SQL syntax provider used for database-specific SQL generation.</param>
    /// <returns>
    /// A <see cref="ColumnDefinition"/> describing the column's database mapping, including type, nullability, primary key, size, constraints, and other settings
    /// as determined by attributes (such as <see cref="NullSettingAttribute"/>, <see cref="SpecialDbTypeAttribute"/>, <see cref="PrimaryKeyColumnAttribute"/>,
    /// <see cref="LengthAttribute"/>, and <see cref="ConstraintAttribute"/>) applied to the property.
    /// </returns>
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

    /// <summary>
    /// Creates a <see cref="ForeignKeyDefinition"/> that describes a foreign key relationship for the specified model property.
    /// </summary>
    /// <param name="modelType">The <see cref="Type"/> of the model declaring the foreign key property.</param>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the property representing the foreign key in the model.</param>
    /// <param name="attribute">The <see cref="ForeignKeyAttribute"/> providing metadata about the referenced table and column.</param>
    /// <param name="columnName">The name of the column in the current (foreign) table that holds the foreign key value.</param>
    /// <param name="tableName">The name of the current (foreign) table containing the foreign key column.</param>
    /// <returns>
    /// A <see cref="ForeignKeyDefinition"/> that specifies the foreign key constraint, including the foreign and primary tables, columns, and referential actions.
    /// </returns>
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

    /// <summary>
    /// Creates an <see cref="IndexDefinition"/> based on the provided model type, property, and index attribute.
    /// </summary>
    /// <param name="modelType">The model <see cref="Type"/> that contains the property.</param>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> representing the property to index.</param>
    /// <param name="attribute">The <see cref="IndexAttribute"/> containing index configuration.</param>
    /// <param name="columnName">The name of the column to be indexed.</param>
    /// <param name="tableName">The name of the table containing the column.</param>
    /// <returns>An <see cref="IndexDefinition"/> describing the index.</returns>
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
