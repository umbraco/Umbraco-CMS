using Umbraco.Web;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext UmbracoContext { get; set; }
    }
}