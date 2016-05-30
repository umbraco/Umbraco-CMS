using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.TestHelpers
{
    class TestFacadeAccessor : IFacadeAccessor
    {
        public IFacade Facade { get; set; }
    }
}
