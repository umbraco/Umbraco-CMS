using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Testing.Objects.AccessorsAndProviders
{
    public class TestSystemDefaultCultureProvider : ISystemDefaultCultureProvider
    {
        public string DefaultCulture { get; set; }
    }
}
