using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.MemberType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Api.Management.Mapping.Member;

public class MemberMapDefinition : ContentMapDefinition<IMember, MemberValueResponseModel, MemberVariantResponseModel>, IMapDefinition
{
    public MemberMapDefinition(PropertyEditorCollection propertyEditorCollection)
        : base(propertyEditorCollection)
    {
    }

    public void DefineMaps(IUmbracoMapper mapper)
        => mapper.Define<IMember, MemberResponseModel>((_, _) => new MemberResponseModel(), Map);

    // Umbraco.Code.MapAll -IsTwoFactorEnabled -Groups -Kind
    private void Map(IMember source, MemberResponseModel target, MapperContext context)
    {
        target.Id = source.Key;
        target.MemberType = context.Map<MemberTypeReferenceResponseModel>(source.ContentType)!;
        target.Values = MapValueViewModels(source.Properties);
        target.Variants = MapVariantViewModels(source);

        target.IsApproved = source.IsApproved;
        target.IsLockedOut = source.IsLockedOut;
        target.Email = source.Email;
        target.Username = source.Username;
        target.FailedPasswordAttempts = source.FailedPasswordAttempts;
        target.LastLoginDate = source.LastLoginDate;
        target.LastLockoutDate = source.LastLockoutDate;
        target.LastPasswordChangeDate = source.LastPasswordChangeDate;
    }
}
