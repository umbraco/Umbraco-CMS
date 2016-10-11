using Umbraco.Web;

namespace Umbraco.Tests
{
    internal class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext UmbracoContext { get; set; }
    }
}