namespace Umbraco.Cms.Infrastructure.PublishedCache.Snap;

internal class GenRef
{
    public readonly GenObj GenObj;

    public GenRef(GenObj genObj) => GenObj = genObj;

    public long Gen => GenObj.Gen;
}
