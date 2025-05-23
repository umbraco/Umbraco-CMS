using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class CommonMapper
{
    private readonly IUserService _userService;

    public CommonMapper(IUserService userService)
    {
        _userService = userService;
    }

    public string? GetOwnerName(IContentBase source, MapperContext context)
        => source.GetCreatorProfile(_userService)?.Name;

    public string? GetCreatorName(IContent source, MapperContext context)
        => source.GetWriterProfile(_userService)?.Name;
}
