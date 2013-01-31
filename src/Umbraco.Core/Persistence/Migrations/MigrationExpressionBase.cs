using System.Linq;

namespace Umbraco.Core.Persistence.Migrations
{
    public abstract class MigrationExpressionBase : IMigrationExpression
    {
        protected MigrationExpressionBase()
        {
        }

        protected MigrationExpressionBase(DatabaseProviders current, DatabaseProviders[] databaseProviders)
        {
            SupportedDatabaseProviders = databaseProviders;
            CurrentDatabaseProvider = current;
        }

        public virtual DatabaseProviders[] SupportedDatabaseProviders { get; private set; }
        public virtual DatabaseProviders CurrentDatabaseProvider { get; private set; }

        public bool IsExpressionSupported()
        {
            if (SupportedDatabaseProviders == null || SupportedDatabaseProviders.Any() == false)
                return true;

            return SupportedDatabaseProviders.Any(x => x == CurrentDatabaseProvider);
        }

        public virtual string Process(Database database)
        {
            return this.ToString();
        }
    }
}