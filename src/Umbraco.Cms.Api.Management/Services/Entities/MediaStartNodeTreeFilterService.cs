using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// User start node tree filter service for media trees.
/// Resolves the current user's media start nodes.
/// </summary>
internal sealed class MediaStartNodeTreeFilterService : UserStartNodeTreeFilterService, IMediaStartNodeTreeFilterService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public MediaStartNodeTreeFilterService(
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        AppCaches appCaches)
        : base(userStartNodeEntitiesService, dataTypeService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    protected override UmbracoObjectTypes[] TreeObjectTypes => [UmbracoObjectTypes.Media];

    /// <inheritdoc />
    protected override int[] CalculateUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateMediaStartNodeIds(_entityService, _appCaches)
           ?? Array.Empty<int>();

    /// <inheritdoc />
    protected override string[] CalculateUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetMediaStartNodePaths(_entityService, _appCaches)
           ?? Array.Empty<string>();
}
