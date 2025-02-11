namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Describes the options available with publishing a content branch for force publishing.
/// </summary>
[Flags]
public enum PublishBranchFilter
{
    /// <summary>
    /// The default behavior is to publish only the published content that has changed.
    /// </summary>
    Default = 0,

    /// <summary>
    /// For publishing a branch, publish all changed content, including content that is not published.
    /// </summary>
    IncludeUnpublished = 1,

    /// <summary>
    /// For publishing a branch, force republishing of all published content, including content that has not changed.
    /// </summary>
    ForceRepublish = 2,

    /// <summary>
    /// For publishing a branch, publish all content, including content that is not published and content that has not changed.
    /// </summary>
    All = IncludeUnpublished | ForceRepublish,
}
