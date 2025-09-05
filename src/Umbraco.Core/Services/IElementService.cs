using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

// TODO ELEMENTS: fully define this interface
public interface IElementService : IPublishableContentService<IElement>
{
    IElement Create(string name, string contentTypeAlias, int userId = Constants.Security.SuperUserId);

    IElement? GetById(Guid key);
}
