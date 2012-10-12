namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    public static class AttributeExtensions
    {
        public static string ToSqlSyntax(this NullSettingAttribute attribute)
        {
            return attribute.NullSetting == NullSettings.Null ? "NULL" : "NOT NULL";
        }

        public static string ToSqlSyntax(this DatabaseTypeAttribute attribute)
        {
            string syntax = string.Empty;
            switch (attribute.DatabaseType)
            {
                case DatabaseTypes.Bool:
                    syntax = "[bit]";
                    break;
                case DatabaseTypes.Ntext:
                    syntax = "[ntext]";
                    break;
                case DatabaseTypes.DateTime:
                    syntax = "[datetime]";
                    break;
                case DatabaseTypes.UniqueIdentifier:
                    syntax = "[uniqueidentifier]";
                    break;
                case DatabaseTypes.SmallInteger:
                    syntax = "[smallint]";
                    break;
                case DatabaseTypes.Integer:
                    syntax = "[int]";
                    break;
                case DatabaseTypes.Nvarchar:
                    syntax = "[nvarchar]";
                    if (attribute.Length > 0)
                        syntax += string.Format(" ({0})", attribute.Length);
                    break;
            }
            return syntax;
        }

        public static string ToSqlSyntax(this PrimaryKeyColumnAttribute attribute)
        {
            string syntax = string.Empty;

            if (attribute.AutoIncrement)
                syntax = "IDENTITY(1, 1)";

            return syntax;
        }

        public static string ToSqlSyntax(this PrimaryKeyColumnAttribute attribute, string tableName, string propertyName)
        {
            string constraintName = string.IsNullOrEmpty(attribute.Name) ? string.Format("PK_{0}", tableName) : attribute.Name;
            string clustered = attribute.Clustered ? "CLUSTERED" : "NONCLUSTERED";
            string syntax = string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY {2} ([{3}])", tableName,
                                          constraintName, clustered, propertyName);

            return syntax;
        }

        public static string ToSqlSyntax(this ConstraintAttribute attribute, string tableName, string propertyName)
        {
            if (!string.IsNullOrEmpty(attribute.Name))
                return attribute.Name;
            
            return string.Format("CONSTRAINT [DF_{0}_{1}] DEFAULT ({2})", tableName, propertyName, attribute.Default);
        }

        public static string ToSqlSyntax(this ForeignKeyAttribute attribute, string tableName, string propertyName)
        {
            var tableNameAttribute = attribute.Type.FirstAttribute<TableNameAttribute>();
            var primaryKeyAttribute = attribute.Type.FirstAttribute<PrimaryKeyAttribute>();
            var referencedTableName = tableNameAttribute.Value;

            string constraintName = string.Format("FK_{0}_{1}", tableName, referencedTableName);
            string syntax =
                string.Format(
                    "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ([{2}]) REFERENCES [{3}] ([{4}])",
                    tableName, constraintName, propertyName, referencedTableName, primaryKeyAttribute.Value);
            return syntax;
        }

        public static string ToSqlSyntax(this IndexAttribute attribute, string tableName, string propertyName)
        {
            string indexType = string.Empty;

            switch (attribute.IndexType)
            {
                case IndexTypes.Clustered:
                    indexType = "CLUSTERED";
                    break;
                case IndexTypes.Nonclustered:
                    indexType = "NONCLUSTERED";
                    break;
                case IndexTypes.PrimaryXml:
                    indexType = "PRIMARYXML";
                    break;
                case IndexTypes.Spartial:
                    indexType = "SPARTIAL";
                    break;
                case IndexTypes.UniqueNonclustered:
                    indexType = "UNIQUENONCLUSTERED";
                    break;
            }
            string name = string.IsNullOrEmpty(attribute.Name) ? string.Format("IX_{0}_{1}", tableName, propertyName) : attribute.Name;
            string syntax = string.Format("CREATE {0} INDEX [{1}] ON [{2}] ([{3}])", indexType, name, tableName, propertyName);
            return syntax;
        }
    }
}