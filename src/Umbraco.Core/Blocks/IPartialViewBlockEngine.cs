using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Blocks;

/// <summary>
///     Defines a contract for rendering block references using partial views.
/// </summary>
public interface IPartialViewBlockEngine
{
    /// <summary>
    ///     Executes the partial view associated with the given block reference and returns the rendered output.
    /// </summary>
    /// <param name="blockReference">The block reference containing content and settings elements to render.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML string.</returns>
    Task<string> ExecuteAsync(IBlockReference<IPublishedElement, IPublishedElement> blockReference);
}
