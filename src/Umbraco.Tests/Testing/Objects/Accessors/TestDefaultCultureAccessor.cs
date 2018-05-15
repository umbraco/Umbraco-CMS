using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class TestDefaultCultureAccessor : IDefaultCultureAccessor
    {
        private string _defaultCulture = string.Empty;

        public string DefaultCulture
        {
            get => _defaultCulture;
            set => _defaultCulture = value ?? string.Empty;
        }
    }
}
