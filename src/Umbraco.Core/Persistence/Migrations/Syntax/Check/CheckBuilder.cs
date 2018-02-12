using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check
{
    public class CheckBuilder : ICheckBuilder
    {
        private readonly IMigrationContext _context;
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly DatabaseProviders[] _databaseProviders;

        public CheckBuilder(IMigrationContext context, ISqlSyntaxProvider sqlSyntax, params DatabaseProviders[] databaseProviders)
        {
            _context = context;
            _sqlSyntax = sqlSyntax;
            _databaseProviders = databaseProviders;
        }
    }
}
