using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="HasScheduleSignProvider"/> class.
    /// </summary>
    public HasScheduleSignProvider(IContentService contentService) => _contentService = contentService;

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
        IEnumerable<Guid> keys = _contentService.GetScheduledContentKeys(items.Select(x => x.Id).ToArray());
        var itemsById = items.ToDictionary(x => x.Id);

        foreach (Guid key in keys)
        {
            if (!itemsById.TryGetValue(key, out TItem? item))
            {
                continue;
            }

            switch (item)
            {
                case DocumentTreeItemResponseModel documentTreeItemResponseModel:
                    documentTreeItemResponseModel.Variants = BuildVariants(documentTreeItemResponseModel.Variants, key);
                    break;

                case DocumentCollectionResponseModel documentCollectionResponseModel:
                    documentCollectionResponseModel.Variants = BuildVariants(documentCollectionResponseModel.Variants, key);
                    break;

                case DocumentItemResponseModel documentItemResponseModel:
                    documentItemResponseModel.Variants = BuildVariants(documentItemResponseModel.Variants, key);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private IEnumerable<DocumentVariantItemResponseModel> BuildVariants(
        IEnumerable<DocumentVariantItemResponseModel> variants, Guid id)
    {
        DocumentVariantItemResponseModel[] listVariants = variants.ToArray();
        if (listVariants.Length == 1)
        {
            DocumentVariantItemResponseModel variant = listVariants[0];
            variant.AddSign(Alias);
            return listVariants;
        }

        IEnumerable<ContentSchedule> schedules = GetSchedule(id);
        foreach (DocumentVariantItemResponseModel variant in listVariants)
        {
            bool isScheduled = schedules.Any(schedule => schedule.Date > DateTime.Now && string.Equals(schedule.Culture, variant.Culture));

            if (isScheduled)
            {
                variant.AddSign(Alias);
            }
        }

        return listVariants;
    }

    private IEnumerable<DocumentVariantResponseModel> BuildVariants(
        IEnumerable<DocumentVariantResponseModel> variants, Guid id)
    {
        DocumentVariantResponseModel[] listVariants = variants.ToArray();
        if (listVariants.Length == 1)
        {
            DocumentVariantResponseModel variant = listVariants[0];
            variant.AddSign(Alias);
            return listVariants;
        }

        IEnumerable<ContentSchedule> schedules = GetSchedule(id);
        var result = new List<DocumentVariantResponseModel>(listVariants.Length);
        foreach (DocumentVariantResponseModel variant in listVariants)
        {
            bool isScheduled = schedules.Any(schedule => schedule.Date > DateTime.Now && string.Equals(schedule.Culture, variant.Culture));

            if (isScheduled)
            {
                variant.AddSign(Alias);
            }
        }

        return result;
    }

    private IEnumerable<ContentSchedule> GetSchedule(Guid id) =>
        _contentService.GetContentScheduleByContentId(id).FullSchedule;
}
