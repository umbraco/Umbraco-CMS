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

    int? ParentId { get; }

    bool ValidatePath(string path, string category);

    bool ValidateRecycleBin(string path, string category);

    bool ValidateProtectedContent(string path, string category);
}
