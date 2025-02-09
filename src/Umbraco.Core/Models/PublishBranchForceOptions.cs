namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Describes the options available with publishing a content branch for force publishing.
/// </summary>
[Flags]
public enum PublishBranchForceOptions
{
    /// <summary>
    /// No force options: the default behavior is to publish only the published content that has changed.
    /// </summary>
    None = 0,

    /// <summary>
    /// For publishing a branch, publish all content, including content that is not published.
    /// </summary>
    PublishUnpublished = 1,

    /// <summary>
    /// For publishing a branch, force republishing of all content, including content that has not changed.
    /// </summary>
    ForceRepublish,

    /// <summary>
    /// For publishing a branch, publish all content, including content that is not published and content that has not changed.
    /// </summary>
    ForceAll = PublishUnpublished | ForceRepublish,
}
