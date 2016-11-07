using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestUmbracoDatabaseAccessor : IUmbracoDatabaseAccessor
    {
        public UmbracoDatabase UmbracoDatabase { get; set; }
    }
}
