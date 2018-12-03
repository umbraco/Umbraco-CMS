using Examine;

namespace Umbraco.Examine
{
    /// <summary>
    /// An extended <see cref="IValueSetValidator"/> for content indexes
    /// </summary>
    public interface IContentValueSetValidator : IValueSetValidator
    {
        bool SupportUnpublishedContent { get; }
        bool SupportProtectedContent { get; }
        int? ParentId { get; }

        bool ValidatePath(string path, string category);
        bool ValidateRecycleBin(string path, string category);
        bool ValidateProtectedContent(string path, string category);
    }
}
