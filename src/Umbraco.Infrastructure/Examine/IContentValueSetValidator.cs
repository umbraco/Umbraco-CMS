using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An extended <see cref="IValueSetValidator" /> for content indexes
/// </summary>
public interface IContentValueSetValidator : IValueSetValidator
{
    /// <summary>
    ///     When set to true the index will only retain published values
    /// </summary>
    /// <remarks>
    ///     Any non-published values will not be put or kept in the index:
    ///     * Deleted, Trashed, non-published Content items
    ///     * non-published Variants
    /// </remarks>
    bool PublishedValuesOnly { get; }

    /// <summary>
    ///     If true, protected content will be indexed otherwise it will not be put or kept in the index
    /// </summary>
    bool SupportProtectedContent { get; }

    /// <summary>
    /// Gets the identifier of the parent content item, if available.
    /// </summary>
    int? ParentId { get; }

    /// <summary>
    /// Determines whether the specified path is valid for the given category.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <param name="category">The category to validate the path against.</param>
    /// <returns><c>true</c> if the path is valid for the category; otherwise, <c>false</c>.</returns>
    bool ValidatePath(string path, string category);

    /// <summary>
    /// Determines whether the specified path and category refer to a recycle bin.
    /// </summary>
    /// <param name="path">The content path to check.</param>
    /// <param name="category">The content category to check.</param>
    /// <returns><c>true</c> if the path and category represent a recycle bin; otherwise, <c>false</c>.</returns>
    bool ValidateRecycleBin(string path, string category);

    /// <summary>
    /// Determines whether the content at the specified path and category should be considered protected.
    /// </summary>
    /// <param name="path">The path of the content to check.</param>
    /// <param name="category">The category of the content to check.</param>
    /// <returns><c>true</c> if the content is considered protected; otherwise, <c>false</c>.</returns>
    bool ValidateProtectedContent(string path, string category);
}
