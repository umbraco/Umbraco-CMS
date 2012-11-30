namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename
{
    public class RenameBuilder : IRenameBuilder
    {
        private readonly IMigrationContext _context;

        public RenameBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}