using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentCollectionPresentationFactory : ContentCollectionPresentationFactory<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>, IDocumentCollectionPresentationFactory
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IEntityService _entityService;

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in V17.")]
    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, IPublicAccessService publicAccessService)
        : this(
              mapper,
              publicAccessService,
              StaticServiceProvider.Instance.GetRequiredService<IEntityService>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, IPublicAccessService publicAccessService, IEntityService entityService)
    : base(mapper)
    {
        _publicAccessService = publicAccessService;
        _entityService = entityService;
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
            item.Ancestors = _entityService.GetPathKeys(matchingContentItem, omitSelf: true)
                .Select(x => new ReferenceByIdModel(x));
        }

        return Task.CompletedTask;
    }
}
