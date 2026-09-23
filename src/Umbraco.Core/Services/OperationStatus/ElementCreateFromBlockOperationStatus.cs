namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the overall outcome of creating a Library element from a block.
/// </summary>
public enum ElementCreateFromBlockOperationStatus
{
    /// <summary>
    ///     The element was created. Individual cultures may still have landed as draft, where they could not be
    ///     published.
    /// </summary>
    Success,

    /// <summary>
    ///     The content item that owns the property could not be found.
    /// </summary>
    OwnerNotFound,

    /// <summary>
    ///     The owner is not of a type that can hold a block property.
    /// </summary>
    OwnerTypeNotSupported,

    /// <summary>
    ///     The owner's stored property value holds no block with the given key.
    /// </summary>
    BlockNotFound,

    /// <summary>
    ///     The element's content type could not be found.
    /// </summary>
    ContentTypeNotFound,

    /// <summary>
    ///     The element's content type is not allowed in the Library.
    /// </summary>
    NotAllowed,

    /// <summary>
    ///     The target Library folder could not be found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     Saving the new element was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     An unexpected failure occurred.
    /// </summary>
    Unknown,
}
