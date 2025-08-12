using Serilog.Core;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
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
    public bool CanProvideSigns<TItem>() =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel);


    /// <inheritdoc/>
    public Task PopulateTreeSignsAsync<TItem>(TItem[] treeItemViewModels, IEnumerable<IEntitySlim> entities)
        where TItem : EntityTreeItemResponseModel, new()
    {
        IEnumerable<Guid> contentKeysScheduledForPublishing = _contentService.GetScheduledContentKeys(treeItemViewModels.Select(x => x.Id));
        foreach (Guid key in contentKeysScheduledForPublishing)
        {
            treeItemViewModels.First(x => x.Id == key).AddSign(Alias);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PopulateCollectionSignsAsync<TItem>(TItem[] collectionItemViewModel)
        where TItem : DocumentCollectionResponseModel, new()
    {
        IEnumerable<Guid> contentKeysScheduledForPublishing = _contentService.GetScheduledContentKeys(collectionItemViewModel.Select(x => x.Id));
        foreach (Guid key in contentKeysScheduledForPublishing)
        {
            collectionItemViewModel.First(x => x.Id == key).AddSign(Alias);
        }

        return Task.CompletedTask;
    }
}
