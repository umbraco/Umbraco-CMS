using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Services.Signs;

/// <summary>
/// Implements a <see cref="ISignProvider"/> that provides signs for entities that have a collection.
/// </summary>
public class HasCollectionSignProvider : ISignProvider
{
    private const string Alias = Constants.Conventions.Signs.Prefix + "HasCollection";

    /// <inheritdoc/>
    public bool CanProvideSigns<TItem>()
        where TItem : IHasSigns =>
        typeof(TItem) == typeof(DocumentTreeItemResponseModel) ||
        typeof(TItem) == typeof(DocumentCollectionResponseModel) ||
        typeof(TItem) == typeof(DocumentItemResponseModel) ||
        typeof(TItem) == typeof(MediaTreeItemResponseModel) ||
        typeof(TItem) == typeof(MediaCollectionResponseModel) ||
        typeof(TItem) == typeof(MediaItemResponseModel);

    /// <inheritdoc/>
    public Task PopulateSignsAsync<TItem>(IEnumerable<TItem> itemViewModels)
        where TItem : IHasSigns
    {
        foreach (TItem item in itemViewModels)
        {
            if (HasCollection(item))
            {
                item.AddSign(Alias);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines if the given view model contains a collection.
    /// </summary>
    private static bool HasCollection(object item) => item switch
    {
        DocumentTreeItemResponseModel { DocumentType.Collection: not null } => true,
        DocumentCollectionResponseModel { DocumentType.Collection: not null } => true,
        DocumentItemResponseModel { DocumentType.Collection: not null } => true,
        MediaTreeItemResponseModel { MediaType.Collection: not null } => true,
        MediaCollectionResponseModel { MediaType.Collection: not null } => true,
        MediaItemResponseModel { MediaType.Collection: not null } => true,
        _ => false,
    };
}
