namespace Umbraco.Cms.Api.Management.ViewModels.Media;

public sealed class MediaUrlInfoResourceSet
{
    public MediaUrlInfoResourceSet(Guid id, IEnumerable<MediaUrlInfo> urlInfos)
    {
        Id = id;
        UrlInfos = urlInfos;
    }

    public Guid Id { get; }

    public IEnumerable<MediaUrlInfo> UrlInfos { get; }
}
