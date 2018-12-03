using Umbraco.Web;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class TestUmbracoContextAccessor : IUmbracoContextAccessor
    {
        public UmbracoContext UmbracoContext { get; set; }
    }
}
