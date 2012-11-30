namespace Umbraco.Core.Persistence.Migrations.Syntax.Update
{
    public class UpdateBuilder : IUpdateBuilder
    {
        private readonly IMigrationContext _context;

        public UpdateBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}