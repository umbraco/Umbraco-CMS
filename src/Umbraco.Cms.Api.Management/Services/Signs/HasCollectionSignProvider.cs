using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for documents that has a collection.
/// </summary>
public class HasCollectionSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "HasCollection";

    /// <inheritdoc/>
    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(MediaTreeItemResponseModel) ||
        typeof(TItem) == typeof(MediaCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel);

/// <inheritdoc/>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            switch (item)
            {
                case DocumentTreeItemResponseModel response:
                    if (response.DocumentType.Collection != null)
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case DocumentCollectionResponseModel response:
                    if (response.DocumentType.Collection != null)
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case MediaTreeItemResponseModel response:
                    if (response.MediaType.Collection != null)
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case MediaCollectionResponseModel response:
                    if (response.MediaType.Collection != null)
                    {
                        item.AddSign(Alias);
                    }

                    break;

                case DocumentItemResponseModel response:
                    if (response.DocumentType.Collection != null)
                    {
                        item.AddSign(Alias);
                    }

                    break;
            }
        }

        return Task.CompletedTask;
    }
}
