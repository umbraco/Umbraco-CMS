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
                 //If current property is a ResultColumn then skip it
                 var resultColumnAttribute = propertyInfo.FirstAttribute<ResultColumnAttribute>();
                 if (resultColumnAttribute != null) continue;

                 //Assumes ExplicitColumn attribute and thus having a ColumnAttribute with the name of the column
                 var columnAttribute = propertyInfo.FirstAttribute<ColumnAttribute>();
                 if (columnAttribute == null) continue;

                 //Creates a column definition and adds it to the collection on the table definition
                 var columnDefinition = new ColumnDefinition
                 {
                     ColumnName = columnAttribute.Name,
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
                             ColumnName = columnAttribute.Name,
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
                         IndexForColumn = columnAttribute.Name
                     };
                     tableDefinition.IndexDefinitions.Add(indexDefinition);
                 }
             }

             return tableDefinition;
         }
    }
}