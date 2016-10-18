namespace Umbraco.Web.Search
{
    public class UmbracoSearcherResolver
    {
        public static IUmbracoSearcher GetInstance(UmbracoHelper umbracoHelper)
        {
            return new ExamineSearcher(umbracoHelper);
        }
    }
}
