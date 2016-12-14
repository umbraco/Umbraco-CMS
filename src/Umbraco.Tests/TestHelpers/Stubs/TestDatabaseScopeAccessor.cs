using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestDatabaseScopeAccessor : IDatabaseScopeAccessor
    {
        public DatabaseScope Scope { get; set; }
    }
}
