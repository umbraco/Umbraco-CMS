using Umbraco.Core.Composing;
using CoreCurrent = Umbraco.Core.Composing.Current;
using WebCurrent = Umbraco.Web.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Routing
{
    public class SiteDomainHelperResolver
    {
        private SiteDomainHelperResolver()
        { }

        public static bool HasCurrent => true;

        public static SiteDomainHelperResolver Current { get; }
            = new SiteDomainHelperResolver();

        public ISiteDomainHelper Helper => WebCurrent.SiteDomainHelper;

        public void SetHelper(ISiteDomainHelper helper)
        {
            CoreCurrent.Container.RegisterSingleton(_ => helper);
        }
    }
}
