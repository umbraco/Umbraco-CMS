using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// User start node tree filter service for element trees.
/// Resolves the current user's element start nodes.
/// </summary>
internal sealed class ElementStartNodeTreeFilterService : UserStartNodeTreeFilterService, IElementStartNodeTreeFilterService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public ElementStartNodeTreeFilterService(
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
    protected override UmbracoObjectTypes[] TreeObjectTypes => [UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer];

    /// <inheritdoc />
    protected override int[] CalculateUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateElementStartNodeIds(_entityService, _appCaches)
           ?? [];

    /// <inheritdoc />
    protected override string[] CalculateUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetElementStartNodePaths(_entityService, _appCaches)
           ?? [];
}
