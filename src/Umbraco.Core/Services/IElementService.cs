using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines the ElementService, which is an easy access to operations involving <see cref="IElement" />
/// </summary>
/// <remarks>
///     Carries no members of its own: every one has been converted to its asynchronous equivalent on
///     <see cref="IPublishableContentService{TContent}" />. It remains as the Element-facing contract
///     that <see cref="ElementService" /> is registered and resolved as.
/// </remarks>
public interface IElementService : IContentServiceBase, IPublishableContentService<IElement>
{
}
