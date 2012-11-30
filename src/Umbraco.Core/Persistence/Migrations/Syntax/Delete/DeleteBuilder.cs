namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete
{
    public class DeleteBuilder : IDeleteBuilder
    {
        private readonly IMigrationContext _context;

        public DeleteBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}