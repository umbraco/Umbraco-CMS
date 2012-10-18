namespace Umbraco.Core.Persistence.DatabaseAnnotations
{
    internal static class AttributeExtensions
    {
        public static string ToSqlSyntax(this NullSettings settings)
        {
            return settings == NullSettings.Null ? "NULL" : "NOT NULL";
        }

        public static string ToSqlSyntax(this NullSettingAttribute attribute)
        {
            return attribute.NullSetting.ToSqlSyntax();
        }

        public static string ToSqlSyntax(this DatabaseTypes databaseTypes, int length)
        {
            string syntax = string.Empty;
            switch (databaseTypes)
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
                case DatabaseTypes.TinyInteger:
                    syntax = "[tinyint]";
                    break;
                case DatabaseTypes.Integer:
                    syntax = "[int]";
                    break;
                case DatabaseTypes.Nchar:
                    syntax = "[nchar]";
                    if (length > 0)
                        syntax += string.Format(" ({0})", length);
                    break;
                case DatabaseTypes.Nvarchar:
                    syntax = "[nvarchar]";
                    if (length > 0)
                        syntax += string.Format(" ({0})", length);
                    break;
            }
            return syntax;
        }

        public static string ToSqlSyntax(this DatabaseTypeAttribute attribute)
        {
            return attribute.DatabaseType.ToSqlSyntax(attribute.Length);
        }

        public static string ToSqlSyntax(this PrimaryKeyColumnAttribute attribute)
        {
            string syntax = string.Empty;

            if (attribute.AutoIncrement)
                syntax = " IDENTITY(1, 1)";

            return syntax;
        }

        public static string ToSqlSyntax(this PrimaryKeyColumnAttribute attribute, string tableName, string propertyName)
        {
            string constraintName = string.IsNullOrEmpty(attribute.Name) ? string.Format("PK_{0}", tableName) : attribute.Name;
            string clustered = attribute.Clustered ? "CLUSTERED" : "NONCLUSTERED";

            if (DatabaseFactory.Current.DatabaseProvider == DatabaseProviders.SqlServerCE)
                clustered = string.Empty;

            string syntax = string.IsNullOrEmpty(attribute.OnColumns)
                                ? string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY {2} ([{3}])",
                                                tableName, constraintName, clustered, propertyName)
                                : string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY {2} ({3})",
                                                tableName, constraintName, clustered, attribute.OnColumns);

            return syntax;
        }

        public static string ToSqlSyntax(this ConstraintAttribute attribute, string tableName, string propertyName)
        {
            if (!string.IsNullOrEmpty(attribute.Name))
                return string.Format("CONSTRAINT [{0}] DEFAULT ({1})", attribute.Name, attribute.Default);
            
            return string.Format("CONSTRAINT [DF_{0}_{1}] DEFAULT ({2})", tableName, propertyName, attribute.Default);
        }

        public static string ToSqlSyntax(this ForeignKeyAttribute attribute, string tableName, string propertyName)
        {
            var tableNameAttribute = attribute.Type.FirstAttribute<TableNameAttribute>();
            var primaryKeyAttribute = attribute.Type.FirstAttribute<PrimaryKeyAttribute>();
            var referencedTableName = tableNameAttribute.Value;

            string constraintName = string.IsNullOrEmpty(attribute.Name)
                                        ? string.Format("FK_{0}_{1}", tableName, referencedTableName)
                                        : attribute.Name;

            string referencedColumn = string.IsNullOrEmpty(attribute.Column)
                                          ? primaryKeyAttribute.Value
                                          : attribute.Column;
            string syntax =
                string.Format(
                    "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ([{2}]) REFERENCES [{3}] ([{4}])",
                    tableName, constraintName, propertyName, referencedTableName, referencedColumn);

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
                case IndexTypes.NonClustered:
                    indexType = "NONCLUSTERED";
                    break;
                case IndexTypes.PrimaryXml:
                    indexType = "PRIMARYXML";
                    break;
                case IndexTypes.Spartial:
                    indexType = "SPARTIAL";
                    break;
                case IndexTypes.UniqueNonClustered:
                    indexType = "UNIQUE NONCLUSTERED";
                    break;
            }
            string name = string.IsNullOrEmpty(attribute.Name) ? string.Format("IX_{0}_{1}", tableName, propertyName) : attribute.Name;

            string syntax = string.IsNullOrEmpty(attribute.ForColumns)
                                ? string.Format("CREATE {0} INDEX [{1}] ON [{2}] ([{3}])", indexType, name, tableName,
                                                propertyName)
                                : string.Format("CREATE {0} INDEX [{1}] ON [{2}] ({3})", indexType, name, tableName,
                                                attribute.ForColumns);
            return syntax;
        }
    }
}