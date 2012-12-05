using System;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions
{
    public static class DefinitionFactory
    {
         public static TableDefinition GetTableDefinition(Type modelType)
         {
             var tableNameAttribute = modelType.FirstAttribute<TableNameAttribute>();
             string tableName = tableNameAttribute.Value;

             var tableDefinition = new TableDefinition { TableName = tableName };
             var objProperties = modelType.GetProperties().ToList();
             foreach (var propertyInfo in objProperties)
             {
                 //If current property has an IgnoreAttribute then skip it
                 var ignoreAttribute = propertyInfo.FirstAttribute<IgnoreAttribute>();
                 if (ignoreAttribute != null) continue;

                 //If current property has a ResultColumnAttribute then skip it
                 var resultColumnAttribute = propertyInfo.FirstAttribute<ResultColumnAttribute>();
                 if (resultColumnAttribute != null) continue;

                 //Looks for ColumnAttribute with the name of the column, which would exist with ExplicitColumns
                 //Otherwise use the name of the property itself as the default convention
                 var columnAttribute = propertyInfo.FirstAttribute<ColumnAttribute>();
                 string columnName = columnAttribute != null ? columnAttribute.Name : propertyInfo.Name;

                 //Use PetaPoco's PrimaryKeyAttribute to set Identity property on TableDefinition
                 var primaryKeyAttribute = modelType.FirstAttribute<PrimaryKeyAttribute>();
                 tableDefinition.IsIdentity = primaryKeyAttribute != null && primaryKeyAttribute.autoIncrement;

                 //Creates a column definition and adds it to the collection on the table definition
                 var columnDefinition = new ColumnDefinition
                 {
                     ColumnName = columnName,
                     PropertyType = propertyInfo.PropertyType
                 };

                 //Look for specific DbType attributed a column
                 var databaseTypeAttribute = propertyInfo.FirstAttribute<SpecialDbTypeAttribute>();
                 if (databaseTypeAttribute != null)
                 {
                     columnDefinition.HasSpecialDbType = true;
                     columnDefinition.DbType = databaseTypeAttribute.DatabaseType;
                 }

                 var lengthAttribute = propertyInfo.FirstAttribute<LengthAttribute>();
                 if(lengthAttribute != null)
                 {
                     columnDefinition.DbTypeLength = lengthAttribute.Length;
                 }

                 //Look for specific Null setting attributed a column
                 var nullSettingAttribute = propertyInfo.FirstAttribute<NullSettingAttribute>();
                 if (nullSettingAttribute != null)
                 {
                     columnDefinition.IsNullable = nullSettingAttribute.NullSetting == NullSettings.Null;
                 }

                 //Look for Primary Key for the current column
                 var primaryKeyColumnAttribute = propertyInfo.FirstAttribute<PrimaryKeyColumnAttribute>();
                 if (primaryKeyColumnAttribute != null)
                 {
                     columnDefinition.IsPrimaryKey = true;
                     columnDefinition.IsPrimaryKeyIdentityColumn = primaryKeyColumnAttribute.AutoIncrement;
                     columnDefinition.IsPrimaryKeyClustered = primaryKeyColumnAttribute.Clustered;
                     columnDefinition.PrimaryKeyName = primaryKeyColumnAttribute.Name ?? string.Empty;
                     columnDefinition.PrimaryKeyColumns = primaryKeyColumnAttribute.OnColumns ?? string.Empty;
                     columnDefinition.PrimaryKeySeeding = primaryKeyColumnAttribute.IdentitySeed;
                 }

                 //Look for Constraint for the current column
                 var constraintAttribute = propertyInfo.FirstAttribute<ConstraintAttribute>();
                 if (constraintAttribute != null)
                 {
                     columnDefinition.ConstraintName = constraintAttribute.Name ?? string.Empty;
                     columnDefinition.ConstraintDefaultValue = constraintAttribute.Default ?? string.Empty;
                 }

                 tableDefinition.ColumnDefinitions.Add(columnDefinition);

                 //Creates a foreignkey definition and adds it to the collection on the table definition
                 var foreignKeyAttributes = propertyInfo.MultipleAttribute<ForeignKeyAttribute>();
                 if (foreignKeyAttributes != null)
                 {
                     foreach (var foreignKeyAttribute in foreignKeyAttributes)
                     {
                         var referencedTable = foreignKeyAttribute.Type.FirstAttribute<TableNameAttribute>();
                         var referencedPrimaryKey = foreignKeyAttribute.Type.FirstAttribute<PrimaryKeyAttribute>();

                         string referencedColumn = string.IsNullOrEmpty(foreignKeyAttribute.Column)
                                           ? referencedPrimaryKey.Value
                                           : foreignKeyAttribute.Column;

                         var foreignKeyDefinition = new ForeignKeyDefinition
                         {
                             ColumnName = columnName,
                             ConstraintName = foreignKeyAttribute.Name,
                             ReferencedColumnName = referencedColumn,
                             ReferencedTableName = referencedTable.Value
                         };
                         tableDefinition.ForeignKeyDefinitions.Add(foreignKeyDefinition);
                     }
                 }

                 //Creates an index definition and adds it to the collection on the table definition
                 var indexAttribute = propertyInfo.FirstAttribute<IndexAttribute>();
                 if (indexAttribute != null)
                 {
                     var indexDefinition = new IndexDefinition
                     {
                         ColumnNames = indexAttribute.ForColumns,
                         IndexName = indexAttribute.Name,
                         IndexType = indexAttribute.IndexType,
                         IndexForColumn = columnName
                     };
                     tableDefinition.IndexDefinitions.Add(indexDefinition);
                 }
             }

             return tableDefinition;
         }
    }
}