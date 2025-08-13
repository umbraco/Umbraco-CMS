using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
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
        typeof(TItem) == typeof(DocumentCollectionResponseModel);

    /// <inheritdoc/>
    public Task PopulateTreeSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
         where TItem : EntityTreeItemResponseModel, IHasSigns => PopulateSigns(itemViewModels);

    /// <inheritdoc/>
    public Task PopulateCollectionSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns => PopulateSigns(itemViewModels);

    private Task PopulateSigns<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        IEnumerable<Guid> contentKeysScheduledForPublishing = _contentService.GetScheduledContentKeys(itemViewModels.Select(x => x.Id));
        foreach (Guid key in contentKeysScheduledForPublishing)
        {
            itemViewModels.First(x => x.Id == key).AddSign(Alias);
        }

        return Task.CompletedTask;
    }
}
