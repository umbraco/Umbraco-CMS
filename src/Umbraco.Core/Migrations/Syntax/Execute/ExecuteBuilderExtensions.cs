using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Syntax.Execute
{
    public static class ExecuteBuilderExtensions
    {
        public static void DropKeysAndIndexes(this IExecuteBuilder execute, string tableName)
        {
            execute.Code(context => DropKeysAndIndexes(context, tableName));
        }

        public static void DropKeysAndIndexes(this IExecuteBuilder execute)
        {
            execute.Code(DropKeysAndIndexes);
        }

        private static string DropKeysAndIndexes(IMigrationContext context, string tableName)
        {
            var local = context.GetLocalMigration();

            // drop keys
            var keys = context.SqlContext.SqlSyntax.GetConstraintsPerTable(context.Database).DistinctBy(x => x.Item2).ToArray();
            foreach (var key in keys.Where(x => x.Item1 == tableName && x.Item2.StartsWith("FK_")))
                local.Delete.ForeignKey(key.Item2).OnTable(key.Item1);
            foreach (var key in keys.Where(x => x.Item1 == tableName && x.Item2.StartsWith("PK_")))
                local.Delete.PrimaryKey(key.Item2).FromTable(key.Item1);

            // drop indexes
            var indexes = context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(context.Database).DistinctBy(x => x.IndexName).ToArray();
            foreach (var index in indexes.Where(x => x.TableName == tableName))
                local.Delete.Index(index.IndexName).OnTable(index.TableName);

            return local.GetSql();
        }

        private static string DropKeysAndIndexes(IMigrationContext context)
        {
            var local = context.GetLocalMigration();

            // drop keys
            var keys = context.SqlContext.SqlSyntax.GetConstraintsPerTable(context.Database).DistinctBy(x => x.Item2).ToArray();
            foreach (var key in keys.Where(x => x.Item2.StartsWith("FK_")))
                local.Delete.ForeignKey(key.Item2).OnTable(key.Item1);
            foreach (var key in keys.Where(x => x.Item2.StartsWith("PK_")))
                local.Delete.PrimaryKey(key.Item2).FromTable(key.Item1);

            // drop indexes
            var indexes = context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(context.Database).DistinctBy(x => x.IndexName).ToArray();
            foreach (var index in indexes)
                local.Delete.Index(index.IndexName).OnTable(index.TableName);

            return local.GetSql();
        }
    }
}
