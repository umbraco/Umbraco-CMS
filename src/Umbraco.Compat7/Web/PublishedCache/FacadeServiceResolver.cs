using WebCurrent = Umbraco.Web.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.PublishedCache
{
    public class FacadeServiceResolver
    {
        private FacadeServiceResolver()
        { }

        public static FacadeServiceResolver Current { get; } = new FacadeServiceResolver();

        public IFacadeService Service => WebCurrent.FacadeService;
    }
}
