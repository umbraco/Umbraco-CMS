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

        foreach (Guid key in keys)
        {
            TItem item = items.First(x => x.Id == key);

            switch (item)
            {
                case DocumentTreeItemResponseModel document:
                {
                    var variants = new List<DocumentVariantItemResponseModel>();
                    if (document.Variants.Count() == 1)
                    {
                        DocumentVariantItemResponseModel variant = document.Variants.First();
                        variant.AddSign(Alias);
                        variants.Add(variant);
                    }
                    else
                    {
                        IEnumerable<ContentSchedule> schedules =
                            _contentService.GetContentScheduleByContentId(key).GetSchedule();
                        IEnumerable<string> culturesWithSchedule = schedules
                            .Select(schedule => schedule.Culture);

                        foreach (DocumentVariantItemResponseModel variant in document.Variants)
                        {
                            if (variant.Culture != null && culturesWithSchedule.Contains(variant.Culture))
                            {
                                variant.AddSign(Alias);
                                variants.Add(variant);
                            }
                        }
                    }

                    document.Variants = variants;
                    break;
                }

                case DocumentCollectionResponseModel document:
                {
                    var variants = new List<DocumentVariantResponseModel>();
                    if (document.Variants.Count() == 1)
                    {
                        DocumentVariantResponseModel variant = document.Variants.First();
                        variant.AddSign(Alias);
                        variants.Add(variant);
                    }
                    else
                    {
                        IEnumerable<ContentSchedule> schedules =
                            _contentService.GetContentScheduleByContentId(key).GetSchedule();
                        IEnumerable<string> culturesWithSchedule = schedules
                            .Select(schedule => schedule.Culture);

                        foreach (DocumentVariantResponseModel variant in document.Variants)
                        {
                            if (variant.Culture != null && culturesWithSchedule.Contains(variant.Culture))
                            {
                                variant.AddSign(Alias);
                                variants.Add(variant);
                            }
                        }
                    }

                    document.Variants = variants;
                    break;
                }

                case DocumentItemResponseModel document:
                {
                    var variants = new List<DocumentVariantItemResponseModel>();
                    if (document.Variants.Count() == 1)
                    {
                        DocumentVariantItemResponseModel variant = document.Variants.First();
                        variant.AddSign(Alias);
                        variants.Add(variant);
                    }
                    else
                    {
                        IEnumerable<ContentSchedule> schedules =
                            _contentService.GetContentScheduleByContentId(key).GetSchedule();
                        IEnumerable<string> culturesWithSchedule = schedules
                            .Select(schedule => schedule.Culture);

                        foreach (DocumentVariantItemResponseModel variant in document.Variants)
                        {
                            if (variant.Culture != null && culturesWithSchedule.Contains(variant.Culture))
                            {
                                variant.AddSign(Alias);
                                variants.Add(variant);
                            }
                        }
                    }

                    document.Variants = variants;
                    break;
                }
            }
        }

        return Task.CompletedTask;
    }
}
