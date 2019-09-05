namespace Umbraco.Web.PublishedCache.NuCache.Snap
{
    internal class GenRef
    {
        public GenRef(GenObj genObj)
        {
            GenObj = genObj;
        }

        public readonly GenObj GenObj;
        public long Gen => GenObj.Gen;
    }
}
