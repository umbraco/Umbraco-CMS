using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A service that can return a list of allowed object types in relation types
/// </summary>
public class AllowedRelationObjectTypesService : IAllowedRelationObjectTypesService
{
    /// <summary>
    /// Gets a list of allowed object types that can be parent/child in a relation type
    /// </summary>
    /// <returns>A list of <see cref="UmbracoObjectTypes"/></returns>
    public IEnumerable<UmbracoObjectTypes> Get() =>
        new[]
        {
            UmbracoObjectTypes.Document,
            UmbracoObjectTypes.Media,
            UmbracoObjectTypes.Member,
            UmbracoObjectTypes.DocumentType,
            UmbracoObjectTypes.MediaType,
            UmbracoObjectTypes.MemberType,
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.MemberGroup,
            UmbracoObjectTypes.ROOT,
            UmbracoObjectTypes.RecycleBin,
        };
}
