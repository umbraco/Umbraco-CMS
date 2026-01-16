namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
///     The saved state of a content item
/// </summary>
// TODO ELEMENTS: move this to ViewModels.Content and rename it to VariantState or ContentVariantState (shared between document and element variants)
public enum DocumentVariantState
{
    /// <summary>
    ///     The item isn't created yet
    /// </summary>
    NotCreated = 1,

    /// <summary>
    ///     The item is saved but isn't published
    /// </summary>
    Draft = 2,

    /// <summary>
    ///     The item is published and there are no pending changes
    /// </summary>
    Published = 3,

    /// <summary>
    ///     The item is published and there are pending changes
    /// </summary>
    PublishedPendingChanges = 4,

    /// <summary>
    ///     The item is in the recycle bin
    /// </summary>
    Trashed = 5,
}
