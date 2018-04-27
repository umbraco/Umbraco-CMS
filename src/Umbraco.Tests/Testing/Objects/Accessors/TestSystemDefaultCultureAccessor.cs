using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class TestSystemDefaultCultureAccessor : ISystemDefaultCultureAccessor
    {
        public string DefaultCulture { get; set; }
    }
}
