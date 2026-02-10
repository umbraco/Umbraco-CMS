using Lucene.Net.Util;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentCollectionPresentationFactory : ContentCollectionPresentationFactory<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>, IDocumentCollectionPresentationFactory
{
    private readonly IPublicAccessService _publicAccessService;
    private readonly IEntityService _entityService;

    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IPublicAccessService publicAccessService, IEntityService entityService, IUserService userService)
        : base(mapper, flagProviders, userService)
    {
        _publicAccessService = publicAccessService;
        _entityService = entityService;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, FlagProviderCollection flagProviders, IPublicAccessService publicAccessService, IEntityService entityService)
        : base(mapper, flagProviders)
    {
        _publicAccessService = publicAccessService;
        _entityService = entityService;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public DocumentCollectionPresentationFactory(IUmbracoMapper mapper, IPublicAccessService publicAccessService, IEntityService entityService)
        : base(mapper)
    {
        _publicAccessService = publicAccessService;
        _entityService = entityService;
    }

    /// <inheritdoc/>
    protected override Task SetUnmappedProperties(ListViewPagedModel<IContent> contentCollection, List<DocumentCollectionResponseModel> collectionResponseModels)
    {
        // Retrieve all public access entries once (single scope) instead of
        // calling IsProtected per item which creates N scopes.
        var protectedNodeIds = new HashSet<int>(
            _publicAccessService.GetAll().Select(entry => entry.ProtectedNodeId));

        // All items in a collection are siblings (same parent), so with omitSelf
        // they share the same ancestor array. Compute once, reuse for all.
        IEnumerable<ReferenceByIdModel>? sharedAncestors = null;

        // Create a lookup of content items by key for efficient matching when looping the response models.
        var contentByKey = contentCollection.Items.Items.ToDictionary(x => x.Key);

        foreach (DocumentCollectionResponseModel item in collectionResponseModels)
        {
            if (contentByKey.TryGetValue(item.Id, out IContent? matchingContentItem) is false)
            {
                continue;
            }

            item.IsProtected = IsProtected(matchingContentItem, protectedNodeIds);
            sharedAncestors ??= _entityService.GetPathKeys(matchingContentItem, omitSelf: true)
                .Select(x => new ReferenceByIdModel(x))
                .ToArray();
            item.Ancestors = sharedAncestors;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks whether a content item is protected by public access, using a pre-fetched
    /// set of protected node IDs.
    /// </summary>
    private static bool IsProtected(IContent content, HashSet<int> protectedNodeIds)
    {
        if (protectedNodeIds.Count == 0)
        {
            return false;
        }

        // Walk the content path from deepest to shallowest, checking for a match.
        int[] pathIds = content.Path.EnsureEndsWith("," + content.Id).GetIdsFromPathReversed();
        foreach (var id in pathIds)
        {
            if (id != Core.Constants.System.Root && protectedNodeIds.Contains(id))
            {
                return true;
            }
        }

        return false;
    }
}
