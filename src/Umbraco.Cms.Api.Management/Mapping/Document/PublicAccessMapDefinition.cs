using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Mapping.Document;

public class PublicAccessMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper) => mapper.Define<PublicAccessRequestModel, PublicAccessEntrySlim>((_, _) => new PublicAccessEntrySlim(), Map);

    // Umbraco.Code.MapAll
    private static void Map(PublicAccessRequestModel source, PublicAccessEntrySlim target, MapperContext context)
    {
        target.ContentId = source.ContentId;
        target.MemberGroupNames = source.MemberGroupNames;
        target.MemberUserNames = source.MemberUserNames;
        target.ErrorPageId = source.ErrorPageId;
        target.LoginPageId = source.LoginPageId;
    }
}
