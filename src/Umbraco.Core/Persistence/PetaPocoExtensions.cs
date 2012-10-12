using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence
{
    public static class PetaPocoExtensions
    {
        public static void CreateTable<T>(this Database db)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, false, tableType);
        }

        public static void CreateTable<T>(this Database db, bool overwrite)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, overwrite, tableType);
        }

        public static void CreateTable(this Database db, bool overwrite, Type modelType)
        {
            var tableNameAttribute = modelType.FirstAttribute<TableNameAttribute>();

            var objProperties = modelType.GetProperties().ToList();
            string tableName = tableNameAttribute.Value;

            var sql = Sql.Builder.Append(string.Format("CREATE TABLE {0} (", tableName));
            var primaryKeyConstraints = new List<Sql>();
            var foreignKeyConstraints = new List<Sql>();
            var indexes = new List<Sql>();

            var last = objProperties.Last();
            foreach (var propertyInfo in objProperties)
            {
                var columnAttribute = propertyInfo.FirstAttribute<ColumnAttribute>();
                if(columnAttribute == null) continue;

                var sb = new StringBuilder();
                sb.AppendFormat("[{0}]", columnAttribute.Name);

                var databaseTypeAttribute = propertyInfo.FirstAttribute<DatabaseTypeAttribute>();
                if (databaseTypeAttribute != null)
                    sb.AppendFormat(" {0}", databaseTypeAttribute.ToSqlSyntax());

                var nullSettingAttribute = propertyInfo.FirstAttribute<NullSettingAttribute>();
                if(nullSettingAttribute != null)
                    sb.AppendFormat(" {0}", nullSettingAttribute.ToSqlSyntax());

                var primaryKeyColumnAttribute = propertyInfo.FirstAttribute<PrimaryKeyColumnAttribute>();
                if(primaryKeyColumnAttribute != null)
                {
                    sb.AppendFormat(" {0}", primaryKeyColumnAttribute.ToSqlSyntax());
                    //Add to list of primary key constraints
                    primaryKeyConstraints.Add(new Sql(primaryKeyColumnAttribute.ToSqlSyntax(tableName, columnAttribute.Name)));
                }

                var constraintAttribute = propertyInfo.FirstAttribute<ConstraintAttribute>();
                if(constraintAttribute != null)
                    sb.AppendFormat(" {0}", constraintAttribute.ToSqlSyntax(tableName, columnAttribute.Name));

                if (propertyInfo != last)
                    sb.Append(", ");

                sql.Append(sb.ToString());

                //Look for foreign keys
                var foreignKeyAttribute = propertyInfo.FirstAttribute<ForeignKeyAttribute>();
                if (foreignKeyAttribute != null)
                {
                    foreignKeyConstraints.Add(new Sql(foreignKeyAttribute.ToSqlSyntax(tableName, columnAttribute.Name)));
                }

                //Look for indexes
                var indexAttribute = propertyInfo.FirstAttribute<IndexAttribute>();
                if(indexAttribute != null)
                {
                    indexes.Add(new Sql(indexAttribute.ToSqlSyntax(tableName, columnAttribute.Name)));
                }
            }

            sql.Append(")");//End

            var tableExist = db.TableExist(tableName);
            if(overwrite && tableExist)
            {
                db.DropTable(tableName);
            }

            if(!tableExist)
            {
                int created = db.Execute(sql);
                foreach (var constraint in primaryKeyConstraints)
                {
                    db.Execute(constraint);
                }
                foreach (var constraint in foreignKeyConstraints)
                {
                    db.Execute(constraint);
                }
                foreach (var index in indexes)
                {
                    db.Execute(index);
                }
            }

#if DEBUG
            Console.WriteLine(sql.SQL);
            foreach (var constraint in primaryKeyConstraints)
            {
                Console.WriteLine(constraint.SQL);
            }
            foreach (var constraint in foreignKeyConstraints)
            {
                Console.WriteLine(constraint.SQL);
            }
            foreach (var index in indexes)
            {
                Console.WriteLine(index.SQL);
            }
#endif
        }

        public static void DropTable<T>(this Database db)
            where T : new()
        {
            Type type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    string.Format(
                        "The Type '{0}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.",
                        type.Name));

            string tableName = tableNameAttribute.Value;
            DropTable(db, tableName);
        }

        public static void DropTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format("DROP TABLE {0}", tableName));
            db.Execute(sql);
        }

        public static bool TableExist(this Database db, string tableName)
        {
            var scalar =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName",
                                       new {tableName = tableName});

            return scalar > 0;
        }
    }
}