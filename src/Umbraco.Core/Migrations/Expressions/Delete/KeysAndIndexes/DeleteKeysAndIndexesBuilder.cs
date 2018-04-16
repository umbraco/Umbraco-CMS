using System.Linq;
using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Expressions.Delete.KeysAndIndexes
{
    public class DeleteKeysAndIndexesBuilder : IExecutableBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public DeleteKeysAndIndexesBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public string TableName { get; set; }

        /// <inheritdoc />
        public void Do()
        {
            if (TableName == null)
            {
                // drop keys
                var keys = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToArray();
                foreach (var key in keys.Where(x => x.Item2.StartsWith("FK_")))
                    Delete.ForeignKey(key.Item2).OnTable(key.Item1).Do();
                foreach (var key in keys.Where(x => x.Item2.StartsWith("PK_")))
                    Delete.PrimaryKey(key.Item2).FromTable(key.Item1).Do();

                // drop indexes
                var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToArray();
                foreach (var index in indexes)
                    Delete.Index(index.IndexName).OnTable(index.TableName).Do();
            }
            else
            {
                // drop keys
                var keys = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToArray();
                foreach (var key in keys.Where(x => x.Item1 == TableName && x.Item2.StartsWith("FK_")))
                    Delete.ForeignKey(key.Item2).OnTable(key.Item1).Do();
                foreach (var key in keys.Where(x => x.Item1 == TableName && x.Item2.StartsWith("PK_")))
                    Delete.PrimaryKey(key.Item2).FromTable(key.Item1).Do();

                // drop indexes
                var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToArray();
                foreach (var index in indexes.Where(x => x.TableName == TableName))
                    Delete.Index(index.IndexName).OnTable(index.TableName).Do();
            }
        }

        private IDeleteBuilder Delete => new DeleteBuilder(_context);
    }
}
