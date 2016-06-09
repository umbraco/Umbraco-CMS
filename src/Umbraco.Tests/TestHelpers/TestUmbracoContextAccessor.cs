using Umbraco.Web;

namespace Umbraco.Tests
{
    class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext UmbracoContext { get; set; }
    }
}