using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
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
                case DocumentTreeItemResponseModel docTree:
                    docTree.Variants = BuildVariants(docTree.Variants, key);
                    break;

                case DocumentCollectionResponseModel docColl:
                    docColl.Variants = BuildVariants(docColl.Variants, key);
                    break;

                case DocumentItemResponseModel docItem:
                    docItem.Variants = BuildVariants(docItem.Variants, key);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private List<DocumentVariantItemResponseModel> BuildVariants(IEnumerable<DocumentVariantItemResponseModel> variants, Guid id)
    {
        var listVariants = variants.ToList();
        if (listVariants.Count == 1)
        {
            DocumentVariantItemResponseModel variant = listVariants[0];
            variant.AddSign(Alias);
            return [variant];
        }

        IEnumerable<string> cultures = GetCulturesWithSchedule(id);
        var result = new List<DocumentVariantItemResponseModel>(listVariants.Count);
        foreach (DocumentVariantItemResponseModel variant in listVariants)
        {
            if (variant.Culture != null && cultures.Contains(variant.Culture))
            {
                variant.AddSign(Alias);
                result.Add(variant);
            }
        }

        return result;
    }

    private List<DocumentVariantResponseModel> BuildVariants(IEnumerable<DocumentVariantResponseModel> variants, Guid id)
    {
        var listVariants = variants.ToList();
        if (listVariants.Count == 1)
        {
            DocumentVariantResponseModel variant = listVariants[0];
            variant.AddSign(Alias);
            return [variant];
        }

        IEnumerable<string> cultures = GetCulturesWithSchedule(id);
        var result = new List<DocumentVariantResponseModel>(listVariants.Count);
        foreach (DocumentVariantResponseModel variant in listVariants)
        {
            if (variant.Culture != null && cultures.Contains(variant.Culture))
            {
                variant.AddSign(Alias);
                result.Add(variant);
            }
        }

        return result;
    }

    private IEnumerable<string> GetCulturesWithSchedule(Guid id) =>
        _contentService.GetContentScheduleByContentId(id)
            .GetSchedule()
            .Select(s => s.Culture);
}
