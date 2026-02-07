using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

/// <summary>
///     Provides common mapping functions for content-related entities.
/// </summary>
public class CommonMapper
{
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommonMapper" /> class.
    /// </summary>
    /// <param name="userService">The user service for retrieving user profiles.</param>
    public CommonMapper(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    ///     Gets the name of the owner (creator) of the content.
    /// </summary>
    /// <param name="source">The content base entity.</param>
    /// <param name="context">The mapper context.</param>
    /// <returns>The name of the owner, or null if not found.</returns>
    public string? GetOwnerName(IContentBase source, MapperContext context)
        => source.GetCreatorProfile(_userService)?.Name;

    /// <summary>
    ///     Gets the name of the creator (last writer) of the content.
    /// </summary>
    /// <param name="source">The content entity.</param>
    /// <param name="context">The mapper context.</param>
    /// <returns>The name of the creator, or null if not found.</returns>
    public string? GetCreatorName(IContent source, MapperContext context)
        => source.GetWriterProfile(_userService)?.Name;
}
