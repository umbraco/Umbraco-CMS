using Umbraco.Web;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public IUmbracoContext UmbracoContext { get; set; }

        public TestUmbracoContextAccessor()
        {
        }

        public TestUmbracoContextAccessor(IUmbracoContext umbracoContext)
        {
            UmbracoContext = umbracoContext;
        }
    }
}
