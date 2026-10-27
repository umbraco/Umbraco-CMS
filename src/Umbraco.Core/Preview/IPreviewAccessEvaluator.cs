using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Preview;

/// <summary>
/// Determines whether the user that the current preview session belongs to may see the unpublished version of a document.
/// </summary>
/// <remarks>
/// The published content cache consults this before handing out a draft, and serves the published version of any
/// document that is refused.
/// </remarks>
public interface IPreviewAccessEvaluator
{
    /// <summary>
    /// Determines whether the unpublished version of the given document may be shown in the current preview session.
    /// </summary>
    /// <param name="content">The unpublished version of the document.</param>
    /// <returns>
    /// <c>true</c> when no user-bound preview session is active, or when the preview user may browse the document;
    /// otherwise <c>false</c>.
    /// </returns>
    bool CanPreviewDocument(IPublishedContent content);
}
