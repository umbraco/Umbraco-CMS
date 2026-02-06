using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

/// <summary>
///     Validator interface for checking constraints when switching a content type between document and element type modes.
/// </summary>
/// <remarks>
///     Element types are content types that can only be used within block-based property editors
///     (like Block List or Block Grid) and cannot have content nodes created directly.
///     This validator ensures that switching between document and element modes is safe.
/// </remarks>
public interface IElementSwitchValidator
{
    /// <summary>
    ///     Validates whether all ancestor content types have the same element flag as the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns>
    ///     <c>true</c> if all ancestors have the same <see cref="IContentTypeBase.IsElement"/> value
    ///     as the specified content type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This validation ensures consistency in inheritance hierarchies where all types
    ///     should be either document types or element types.
    /// </remarks>
    Task<bool> AncestorsAreAlignedAsync(IContentType contentType);

    /// <summary>
    ///     Validates whether all descendant content types have the same element flag as the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns>
    ///     <c>true</c> if all descendants have the same <see cref="IContentTypeBase.IsElement"/> value
    ///     as the specified content type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This validation ensures consistency in inheritance hierarchies where all types
    ///     should be either document types or element types.
    /// </remarks>
    Task<bool> DescendantsAreAlignedAsync(IContentType contentType);

    /// <summary>
    ///     Validates whether an element type can be converted to a document type by checking
    ///     if it is used in any block structure configurations.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns>
    ///     <c>true</c> if the element type is not used in any block structure configurations
    ///     and can safely be converted to a document type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Element types used in Block List, Block Grid, or other block-based property editors
    ///     cannot be converted to document types while they are still referenced.
    /// </remarks>
    Task<bool> ElementToDocumentNotUsedInBlockStructuresAsync(IContentTypeBase contentType);

    /// <summary>
    ///     Validates whether a document type can be converted to an element type by checking
    ///     if any content nodes exist for the content type.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns>
    ///     <c>true</c> if no content nodes exist for the content type and it can safely
    ///     be converted to an element type; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Document types with existing content cannot be converted to element types
    ///     because element types cannot have directly created content nodes.
    /// </remarks>
    Task<bool> DocumentToElementHasNoContentAsync(IContentTypeBase contentType);
}
