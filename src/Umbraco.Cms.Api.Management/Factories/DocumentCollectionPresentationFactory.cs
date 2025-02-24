using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentCollectionPresentationFactory : ContentCollectionPresentationFactory<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>, IDocumentCollectionPresentationFactory
{
    private readonly IPublicAccessService _publicAccessService;

    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, IPublicAccessService publicAccessService)
        : base(mapper)
    {
        _publicAccessService = publicAccessService;
    }

    protected override Task SetUnmappedProperties(ListViewPagedModel<IContent> contentCollection, List<DocumentCollectionResponseModel> collectionResponseModels)
    {
        foreach (DocumentCollectionResponseModel item in collectionResponseModels)
        {
            IContent? matchingContentItem = contentCollection.Items.Items.FirstOrDefault(x => x.Key == item.Id);
            if (matchingContentItem is null)
            {
                continue;
            }

            item.IsProtected = _publicAccessService.IsProtected(matchingContentItem).Success;
        }

        return Task.CompletedTask;
    }
}
