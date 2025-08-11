using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for documents that are scheduled for publication.
/// </summary>
internal class HasScheduleSignProvider : ISignProvider
{
    private const string Alias = ISignProvider.Prefix + "ScheduledForPublish";

    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HasScheduleSignProvider"/> class.
    /// </summary>
    public HasScheduleSignProvider(IContentService contentService) => _contentService = contentService;

    /// <inheritdoc/>
    public bool CanProvideTreeSigns<TItem>() => typeof(TItem) == typeof(DocumentTreeItemResponseModel);

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
}
