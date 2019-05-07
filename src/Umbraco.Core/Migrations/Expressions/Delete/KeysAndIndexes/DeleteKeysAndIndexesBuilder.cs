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

        public bool DeletePrimaryKey { get; set; }

        public bool DeleteForeignKeys { get; set; }

        public bool DeleteIndexes { get; set; }

        /// <inheritdoc />
        public void Do()
        {
            _context.BuildingExpression = false;

            // drop keys
            if (DeleteForeignKeys || DeletePrimaryKey)
            {
                var keys = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToList();
                if (DeleteForeignKeys)
                    foreach (var key in keys.Where(x => x.Item1 == TableName && x.Item2.StartsWith("FK_")))
                        Delete.ForeignKey(key.Item2).OnTable(key.Item1).Do();
                if (DeletePrimaryKey)
                    foreach (var key in keys.Where(x => x.Item1 == TableName && x.Item2.StartsWith("PK_")))
                        Delete.PrimaryKey(key.Item2).FromTable(key.Item1).Do();
            }

            // drop indexes
            if (DeleteIndexes)
            {
                var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToList();
                foreach (var index in indexes.Where(x => x.TableName == TableName))
                    Delete.Index(index.IndexName).OnTable(index.TableName).Do();
            }
        }

        private IDeleteBuilder Delete => new DeleteBuilder(_context);
    }
}
