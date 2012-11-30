namespace Umbraco.Core.Persistence.Migrations.Syntax.Schema
{
    public class SchemaBuilder : ISchemaBuilder
    {
        private readonly IMigrationContext _context;

        public SchemaBuilder(IMigrationContext context)
        {
            _context = context;
        }
    }
}