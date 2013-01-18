using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.Migrations.Syntax.Rename;

namespace Umbraco.Core.Persistence.Migrations.Syntax.IfDatabase
{
    public class IfDatabaseBuilder : IIfDatabaseBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseProviders[] _databaseProviders;

        public IfDatabaseBuilder(IMigrationContext context, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _databaseProviders = databaseProviders;
        }

        public ICreateBuilder Create
        {
            get { return new CreateBuilder(_context, _databaseProviders); }
        }

        public IExecuteBuilder Execute
        {
            get { return new ExecuteBuilder(_context, _databaseProviders); }
        }

        public IDeleteBuilder Delete
        {
            get { return new DeleteBuilder(_context, _databaseProviders); }
        }

        public IRenameBuilder Rename
        {
            get { return new RenameBuilder(_context, _databaseProviders); }
        }
    }
}