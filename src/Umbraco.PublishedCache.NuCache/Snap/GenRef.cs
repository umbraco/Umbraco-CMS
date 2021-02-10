namespace Umbraco.Cms.Infrastructure.PublishedCache.Snap
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
