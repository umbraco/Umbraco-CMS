using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Preview;

/// <summary>
/// Determines whether the user that the current preview session belongs to may see the unpublished version of a
/// document or element.
/// </summary>
/// <remarks>
/// The published caches consult this before handing out a draft, and serve the published version of anything that
/// is refused.
/// </remarks>
public interface IPreviewAccessEvaluator
{
    /// <summary>
    /// Determines whether the unpublished version of the given document may be shown in the current preview session.
    /// </summary>
    /// <param name="content">The unpublished version of the document.</param>
    /// <returns>
    /// <c>true</c> when no preview session is active, or when the preview user may browse the document; otherwise
    /// <c>false</c>, including in a preview session whose user can't be resolved, or is disabled or locked out.
    /// </returns>
    bool CanPreviewDocument(IPublishedContent content);

    /// <summary>
    /// Determines whether the unpublished version of the given element may be shown in the current preview session.
    /// </summary>
    /// <param name="element">The unpublished version of the element.</param>
    /// <returns>
    /// <c>true</c> when no preview session is active, or when the preview user may browse the element; otherwise
    /// <c>false</c>, including in a preview session whose user can't be resolved, or is disabled or locked out.
    /// </returns>
    bool CanPreviewElement(IPublishedElement element);
}
