using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Signs;

public class HasPendingChangesSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "PendingChanges";

    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HasPendingChangesSignProvider"/> class.
    /// </summary>
    public HasPendingChangesSignProvider(IContentService contentService) => _contentService = contentService;

    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel);

    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            IContent? content = _contentService.GetById(item.Id);
            if (content == null)
            {
                continue;
            }

            DocumentVariantState state = DocumentVariantStateHelper.GetState(content, null);

            if (state == DocumentVariantState.PublishedPendingChanges)
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }
}
