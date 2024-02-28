using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using UserProfile = Umbraco.Cms.Core.Models.ContentEditing.UserProfile;

namespace Umbraco.Cms.Core.Models.Mapping;

public class CommonMapper
{
    private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IUserService _userService;

    public CommonMapper(
        IUserService userService,
        IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
        ILocalizedTextService localizedTextService)
    {
        _userService = userService;
        _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        _localizedTextService = localizedTextService;
    }

    public UserProfile? GetOwner(IContentBase source, MapperContext context)
    {
        IProfile? profile = source.GetCreatorProfile(_userService);
        return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
    }

    public UserProfile? GetCreator(IContent source, MapperContext context)
    {
        IProfile? profile = source.GetWriterProfile(_userService);
        return profile == null ? null : context.Map<IProfile, UserProfile>(profile);
    }

    public ContentTypeBasic? GetContentType(IContentBase source, MapperContext context)
    {
        IContentTypeComposition? contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);
        ContentTypeBasic? contentTypeBasic = context.Map<IContentTypeComposition, ContentTypeBasic>(contentType);
        return contentTypeBasic;
    }
}
