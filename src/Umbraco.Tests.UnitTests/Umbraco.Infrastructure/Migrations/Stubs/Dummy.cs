using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    /// <summary>
    /// This is just a dummy class that is used to ensure that implementations
    /// of IMigration is not found if it doesn't have the MigrationAttribute (like this class).
    /// </summary>
    public class Dummy : IMigration
    {
        public void Migrate()
        {
            throw new System.NotImplementedException();
        }
    }
}
