using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// The states of a content item.
    /// </summary>
    public enum PublishedState
    {
        // when a content item is loaded, its state is one of those two:

        /// <summary>
        /// The content item is published.
        /// </summary>
        Published,

        /// <summary>
        /// The content item is not published.
        /// </summary>
        /// <remarks>Also: the version is being saved, in order to register changes
        /// made to an unpublished version of the content.</remarks>
        Unpublished,

        // when it is saved, its state can also be one of those:

        // fixme
        // what we can do is:
        // - save changes to unpublished data (update current version)
        // - save changes resulting from a publish (spawn a version)
        // - save changes resulting from unpublishing (spawn a version)
        //
        // ContentRepository.CreateVersion
        //  only if uDocument.Published already, and Publishing
        //    ie updating what's already published
        //  creates a new uContentVersion, uDocumentVersion
        //    by copying everything from the existing (current) ones
        //    becomes the current ones
        //  then
        //    move all non-published uPropertyData to the new version
        //    create uPropertyData for new published values
        //  update/return the content w/new version
        //
        // saving is just updating draft
        // publishing is described above
        // un-publishing is ...
        //   also create a version to remember what was published
        //   and then on the new version, there is no 'new published values'

        /// <summary>
        /// The version is being saved, in order to register changes made to a published content.
        /// </summary>
        /// <remarks>The <value>Saving</value> state is transitional. Once the version
        /// is saved, its state changes to <value>Unpublished</value>.</remarks>
        Saving,

        /// <summary>
        /// The version is being saved, in order to publish the content.
        /// </summary>
        /// <remarks>The <value>Publishing</value> state is transitional. Once the version
        /// is saved, its state changes to <value>Published</value>. The content is published,
        /// and all other versions are unpublished.</remarks>
        Publishing,

        /// <summary>
        /// The version is being saved, in order to unpublish the content.
        /// </summary>
        /// <remarks>The <value>Unpublishing</value> state is transitional. Once the version
        /// is saved, its state changes to <value>Unpublished</value>. The content and all
        /// other versions are unpublished.</remarks>
        Unpublishing

    }
}
