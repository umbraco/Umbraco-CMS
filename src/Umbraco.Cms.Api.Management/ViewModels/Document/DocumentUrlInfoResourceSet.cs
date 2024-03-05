namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public sealed class DocumentUrlInfoResourceSet
{
    public DocumentUrlInfoResourceSet(Guid id, IEnumerable<DocumentUrlInfo> urlInfos)
    {
        Id = id;
        UrlInfos = urlInfos;
    }

    public Guid Id { get; }

    public IEnumerable<DocumentUrlInfo> UrlInfos { get; }
}
