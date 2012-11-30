namespace Umbraco.Core.Persistence.Migrations.Syntax.Create
{
    public class CreateBuilder : ICreateBuilder
    {
        private readonly IMigrationContext _context;

        public CreateBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}