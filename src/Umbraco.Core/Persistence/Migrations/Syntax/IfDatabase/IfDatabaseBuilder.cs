using NPoco;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;
using Umbraco.Core.Persistence.Migrations.Syntax.Update;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase
{
    public class IfDatabaseBuilder : IIfDatabaseBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public IfDatabaseBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public ICreateBuilder Create => new CreateBuilder(_context, _supportedDatabaseTypes);

        public IExecuteBuilder Execute => new ExecuteBuilder(_context, _supportedDatabaseTypes);

        public IDeleteBuilder Delete => new DeleteBuilder(_context, _supportedDatabaseTypes);

        public IRenameBuilder Rename => new RenameBuilder(_context, _supportedDatabaseTypes);

        public IUpdateBuilder Update => new UpdateBuilder(_context, _supportedDatabaseTypes);
    }
}