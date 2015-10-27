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
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public IfDatabaseBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }

        public ICreateBuilder Create
        {
            get { return new CreateBuilder(_context, _sqlSyntax, _databaseProviders); }
        }

        public IExecuteBuilder Execute
        {
            get { return new ExecuteBuilder(_context, _sqlSyntax, _databaseProviders); }
        }

        public IDeleteBuilder Delete
        {
            get { return new DeleteBuilder(_context, _sqlSyntax, _databaseProviders); }
        }

        public IRenameBuilder Rename
        {
            get { return new RenameBuilder(_context, _sqlSyntax, _databaseProviders); }
        }

        public IUpdateBuilder Update
        {
            get { return new UpdateBuilder(_context, _sqlSyntax, _databaseProviders); }
        }
    }
}