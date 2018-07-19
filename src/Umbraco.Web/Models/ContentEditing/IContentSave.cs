using Umbraco.Core.Models;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// An interface exposes the shared parts of content, media, members that we use during model binding in order to share logic
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    internal interface IContentSave<TPersisted> : IHaveUploadedFiles
        where TPersisted : IContentBase
    {
        /// <summary>
        /// The action to perform when saving this content item
        /// </summary>
        ContentSaveAction Action { get; set; }

        /// <summary>
        /// The real persisted content object - used during inbound model binding
        /// </summary>
        /// <remarks>
        /// This is not used for outgoing model information.
        /// </remarks>
        TPersisted PersistedContent { get; set; }

        /// <summary>
        /// The DTO object used to gather all required content data including data type information etc... for use with validation - used during inbound model binding
        /// </summary>
        /// <remarks>
        /// We basically use this object to hydrate all required data from the database into one object so we can validate everything we need
        /// instead of having to look up all the data individually.
        /// This is not used for outgoing model information.
        /// </remarks>
        ContentItemDto<TPersisted> ContentDto { get; set; }
    }
}