using Umbraco.Core.Persistence;

namespace Umbraco.Tests.TestHelpers
{
    internal class TestUmbracoDatabaseAccessor : IUmbracoDatabaseAccessor
    {
        public UmbracoDatabase UmbracoDatabase { get; set; }
    }
}
