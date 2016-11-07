using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestFacadeAccessor : IFacadeAccessor
    {
        public IFacade Facade { get; set; }
    }
}
