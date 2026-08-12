using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Provides flags for documents that are scheduled for publication.
/// </summary>
internal class HasDocumentScheduleFlagProvider : HasScheduleFlagProviderBase
{
    private readonly IContentService _contentService;

    public HasDocumentScheduleFlagProvider(IContentService contentService, TimeProvider timeProvider)
        : base(timeProvider)
    {
        _contentService = contentService;
    }

    /// <inheritdoc/>
    public override bool CanProvideFlags<TItem>() =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel);

    /// <inheritdoc/>
    protected override IDictionary<Guid, IEnumerable<ContentSchedule>> GetSchedulesByKeys(Guid[] keys)
        => _contentService.GetContentSchedulesByKeys(keys);

    /// <inheritdoc/>
    protected override void PopulateItemFlags<TItem>(TItem item, ContentSchedule[] releaseSchedules)
    {
        switch (item)
        {
            case DocumentTreeItemResponseModel m:
                m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                break;
            case DocumentCollectionResponseModel m:
                m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                break;
            case DocumentItemResponseModel m:
                m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                break;
        }
    }
}
