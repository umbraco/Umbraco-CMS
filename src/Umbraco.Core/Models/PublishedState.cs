namespace Umbraco.Cms.Core.Models;

/// <summary>
///     The states of a content item.
/// </summary>
public enum PublishedState
{
    // versions management in repo:
    //
    // - published = the content is published
    //     repo: saving draft values
    //     update current version (draft) values
    //
    // - unpublished = the content is not published
    //     repo: saving draft values
    //     update current version (draft) values
    //
    // - publishing = the content is being published (transitory)
    //     if currently published:
    //       delete all draft values from current version, not current anymore
    //       create new version with published+draft values
    //
    // - unpublishing = the content is being unpublished (transitory)
    //     if currently published (just in case):
    //       delete all draft values from current version, not current anymore
    //       create new version with published+draft values (should be managed by service)

    // when a content item is loaded, its state is one of those two:

    /// <summary>
    ///     The content item is published.
    /// </summary>
    Published,

    // also: handled over to repo to save draft values for a published content

    /// <summary>
    ///     The content item is not published.
    /// </summary>
    Unpublished,

    // also: handled over to repo to save draft values for an unpublished content

    // when it is handled over to the repository, its state can also be one of those:

    /// <summary>
    ///     The version is being saved, in order to publish the content.
    /// </summary>
    /// <remarks>
    ///     The
    ///     <value>Publishing</value>
    ///     state is transitional. Once the version
    ///     is saved, its state changes to
    ///     <value>Published</value>
    ///     .
    /// </remarks>
    Publishing,

    /// <summary>
    ///     The version is being saved, in order to unpublish the content.
    /// </summary>
    /// <remarks>
    ///     The
    ///     <value>Unpublishing</value>
    ///     state is transitional. Once the version
    ///     is saved, its state changes to
    ///     <value>Unpublished</value>
    ///     .
    /// </remarks>
    Unpublishing,
}
