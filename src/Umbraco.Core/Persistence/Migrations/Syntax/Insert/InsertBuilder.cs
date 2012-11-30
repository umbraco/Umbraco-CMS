namespace Umbraco.Core.Persistence.Migrations.Syntax.Insert
{
    public class InsertBuilder : IInsertBuilder
    {
        private readonly IMigrationContext _context;

        public InsertBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}