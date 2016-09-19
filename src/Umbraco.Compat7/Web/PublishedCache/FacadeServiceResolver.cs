using WebCurrent = Umbraco.Web.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.PublishedCache
{
    public class FacadeServiceResolver
    {
        public static FacadeServiceResolver Current { get; set; } = new FacadeServiceResolver();

        public IFacadeService Service => WebCurrent.FacadeService;
    }
}
