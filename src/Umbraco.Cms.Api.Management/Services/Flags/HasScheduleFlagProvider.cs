using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Services.Flags;

/// <summary>
/// Implements a <see cref="IFlagProvider"/> that provides flags for documents and elements that are scheduled for publication.
/// </summary>
internal class HasScheduleFlagProvider : IFlagProvider
{
    private const string Alias = Constants.Conventions.Flags.Prefix + "ScheduledForPublish";

    private readonly IContentService _contentService;
    private readonly IElementService _elementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HasScheduleFlagProvider"/> class.
    /// </summary>
    public HasScheduleFlagProvider(IContentService contentService, IElementService elementService)
    {
        _contentService = contentService;
        _elementService = elementService;
    }

    /// <inheritdoc/>
    public bool CanProvideFlags<TItem>()
        where TItem : IHasFlags =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel) ||
        typeof(TItem) == typeof(ElementTreeItemResponseModel) ||
        typeof(TItem) == typeof(ElementItemResponseModel);

    /// <inheritdoc/>
    public Task PopulateFlagsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasFlags
    {
        Func<Guid, ContentScheduleCollection> getSchedules = typeof(TItem) switch
        {
            { } t when t == typeof(DocumentTreeItemResponseModel)
                       || t == typeof(DocumentCollectionResponseModel)
                       || t == typeof(DocumentItemResponseModel)
                => id => _contentService.GetContentScheduleByContentId(id),
            { } t when t == typeof(ElementTreeItemResponseModel)
                       || t == typeof(ElementItemResponseModel)
                => id => _elementService.GetContentScheduleByContentId(id),
            _ => throw new NotSupportedException($"Type {typeof(TItem)} is not supported by this flag provider."),
        };

        foreach (TItem item in items)
        {
            ContentSchedule[] releaseSchedules = getSchedules(item.Id).FullSchedule
                .Where(s => s.Action == ContentScheduleAction.Release)
                .ToArray();

            if (releaseSchedules.Length == 0)
            {
                continue;
            }

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
                case ElementTreeItemResponseModel m:
                    m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                    break;
                case ElementItemResponseModel m:
                    m.Variants = PopulateVariants(m.Variants, releaseSchedules, v => v.Culture);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private IEnumerable<TVariant> PopulateVariants<TVariant>(
        IEnumerable<TVariant> variants,
        ContentSchedule[] schedules,
        Func<TVariant, string?> getCulture)
        where TVariant : IHasFlags
    {
        TVariant[] variantsArray = variants.ToArray();
        if (variantsArray.Length == 1)
        {
            variantsArray[0].AddFlag(Alias);
            return variantsArray;
        }

        foreach (TVariant variant in variantsArray)
        {
            var culture = getCulture(variant);
            ContentSchedule? schedule = schedules.FirstOrDefault(x => x.Culture == culture);
            if (schedule is not null && schedule.Date > DateTime.Now && string.Equals(schedule.Culture, culture))
            {
                variant.AddFlag(Alias);
            }
        }

        return variantsArray;
    }
}
