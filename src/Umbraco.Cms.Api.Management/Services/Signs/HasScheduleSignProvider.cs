using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for documents that are scheduled for publication.
/// </summary>
internal class HasScheduleSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "ScheduledForPublish";

    private readonly IContentService _contentService;
    private readonly IIdKeyMap _keyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="HasScheduleSignProvider"/> class.
    /// </summary>
    public HasScheduleSignProvider(IContentService contentService, IIdKeyMap keyMap)
    {
        _contentService = contentService;
        _keyMap = keyMap;
    }

    /// <inheritdoc/>
    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel);

    /// <inheritdoc/>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> items)
        where TItem : IHasSigns
    {
        IDictionary<int, IEnumerable<ContentSchedule>> schedules = _contentService.GetContentSchedulesByIds(items.Select(x => x.Id).ToArray());
        foreach (TItem item in items)
        {
            Attempt<int> itemId = _keyMap.GetIdForKey(item.Id, UmbracoObjectTypes.Document);
            if (itemId.Success is false)
            {
                continue;
            }

            if (!schedules.TryGetValue(itemId.Result, out IEnumerable<ContentSchedule>? contentSchedules))
            {
                continue;
            }

            switch (item)
            {
                case DocumentTreeItemResponseModel documentTreeItemResponseModel:
                    documentTreeItemResponseModel.Variants = PopulateVariants(documentTreeItemResponseModel.Variants, contentSchedules);
                    break;

                case DocumentCollectionResponseModel documentCollectionResponseModel:
                    documentCollectionResponseModel.Variants = PopulateVariants(documentCollectionResponseModel.Variants, contentSchedules);
                    break;

                case DocumentItemResponseModel documentItemResponseModel:
                    documentItemResponseModel.Variants = PopulateVariants(documentItemResponseModel.Variants, contentSchedules);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private IEnumerable<DocumentVariantItemResponseModel> PopulateVariants(
        IEnumerable<DocumentVariantItemResponseModel> variants, IEnumerable<ContentSchedule> schedules)
    {
        DocumentVariantItemResponseModel[] variantsArray = variants.ToArray();
        if (variantsArray.Length == 1)
        {
            DocumentVariantItemResponseModel variant = variantsArray[0];
            variant.AddSign(Alias);
            return variantsArray;
        }

        foreach (DocumentVariantItemResponseModel variant in variantsArray)
        {
            ContentSchedule? schedule = schedules.FirstOrDefault(x => x.Culture == variant.Culture);
            bool isScheduled = schedule != null && schedule.Date > DateTime.Now && string.Equals(schedule.Culture, variant.Culture);

            if (isScheduled)
            {
                variant.AddSign(Alias);
            }
        }

        return variantsArray;
    }

    private IEnumerable<DocumentVariantResponseModel> PopulateVariants(
        IEnumerable<DocumentVariantResponseModel> variants, IEnumerable<ContentSchedule> schedules)
    {
        DocumentVariantResponseModel[] variantsArray = variants.ToArray();
        if (variantsArray.Length == 1)
        {
            DocumentVariantResponseModel variant = variantsArray[0];
            variant.AddSign(Alias);
            return variantsArray;
        }

        foreach (DocumentVariantResponseModel variant in variantsArray)
        {
            ContentSchedule? schedule = schedules.FirstOrDefault(x => x.Culture == variant.Culture);
            bool isScheduled = schedule != null && schedule.Date > DateTime.Now && string.Equals(schedule.Culture, variant.Culture);

            if (isScheduled)
            {
                variant.AddSign(Alias);
            }
        }

        return variantsArray;
    }
}
