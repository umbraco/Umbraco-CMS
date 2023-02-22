using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IAllowedRelationObjectTypesService
{
    IEnumerable<UmbracoObjectTypes> Get();
}
