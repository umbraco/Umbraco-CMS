namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     An interface exposes the shared parts of content, media, members that we use during model binding in order to share
///     logic
/// </summary>
/// <typeparam name="TPersisted"></typeparam>
public interface IContentSave<TPersisted> : IHaveUploadedFiles
    where TPersisted : IContentBase
{
    /// <summary>
    ///     The action to perform when saving this content item
    /// </summary>
    ContentSaveAction Action { get; set; }

    /// <summary>
    ///     The real persisted content object - used during inbound model binding
    /// </summary>
    /// <remarks>
    ///     This is not used for outgoing model information.
    /// </remarks>
    TPersisted PersistedContent { get; set; }
}
