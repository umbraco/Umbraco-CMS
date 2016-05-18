using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// The <c>IContent</c> states of a content version.
    /// </summary>
    public enum PublishedState
    {
        // when a content version is loaded, its state is one of those two:

        /// <summary>
        /// The version is published.
        /// </summary>
        Published,

        /// <summary>
        /// The version is not published.
        /// </summary>
        /// <remarks>Also: the version is being saved, in order to register changes
        /// made to an unpublished version of the content.</remarks>
        Unpublished,

        // legacy - remove
        [Obsolete("kill!", true)]
        Saved,

        // when it is saved, its state can also be one of those:

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