using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: fully define this interface
public interface IElementService : IPublishableContentService<IElement>
{
    /// <summary>
    ///     Creates an element.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <param name="contentTypeAlias">The content type alias.</param>
    /// <param name="userId">The identifier of the user performing the action.</param>
    /// <returns>The created element.</returns>
    IElement Create(string name, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Gets elements.
    /// </summary>
    /// <param name="keys">The identifiers of the elements.</param>
    /// <returns>The elements.</returns>
    IEnumerable<IElement> GetByIds(IEnumerable<Guid> keys);
}
