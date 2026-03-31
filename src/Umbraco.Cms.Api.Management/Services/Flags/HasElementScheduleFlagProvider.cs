using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Provides flags for elements that are scheduled for publication.
/// </summary>
internal class HasElementScheduleFlagProvider : HasScheduleFlagProviderBase
{
    private readonly IElementService _elementService;

    public HasElementScheduleFlagProvider(IElementService elementService, TimeProvider timeProvider)
        : base(timeProvider)
    {
        _elementService = elementService;
    }

    /// <inheritdoc/>
    public override bool CanProvideFlags<TItem>() =>
        typeof(TItem) == typeof(ElementTreeItemResponseModel) ||
        typeof(TItem) == typeof(ElementItemResponseModel);

    /// <inheritdoc/>
    protected override IDictionary<Guid, IEnumerable<ContentSchedule>> GetSchedulesByKeys(Guid[] keys)
        => _elementService.GetContentSchedulesByKeys(keys);

    /// <inheritdoc/>
    protected override void PopulateItemFlags<TItem>(TItem item, ContentSchedule[] releaseSchedules)
    {
        switch (item)
        {
            case ElementTreeItemResponseModel m:
                m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                break;
            case ElementItemResponseModel m:
                m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                break;
        }
    }
}
