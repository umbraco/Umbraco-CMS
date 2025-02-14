using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentCollectionPresentationFactory : ContentCollectionPresentationFactory<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>, IDocumentCollectionPresentationFactory
{
    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper)
        : base(mapper)
    {
    }

    protected override Task SetUnmappedProperties(List<DocumentCollectionResponseModel> collectionResponseModels)
    {
        // TODO: Map IsProtected

        return Task.CompletedTask;
    }
}
