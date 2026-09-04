using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// User start node tree filter service for document blueprint trees.
/// Resolves the current user's document blueprint start nodes.
/// </summary>
internal sealed class DocumentBlueprintStartNodeTreeFilterService : UserStartNodeTreeFilterService, IDocumentBlueprintStartNodeTreeFilterService
{
    private static readonly UmbracoObjectTypes[] _treeObjectTypes =
        [UmbracoObjectTypes.DocumentBlueprint, UmbracoObjectTypes.DocumentBlueprintContainer];

    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public DocumentBlueprintStartNodeTreeFilterService(
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
    protected override UmbracoObjectTypes[] TreeObjectTypes => _treeObjectTypes;

    /// <inheritdoc />
    protected override int[] CalculateUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateDocumentBlueprintStartNodeIds(_entityService, _appCaches)
           ?? [];

    /// <inheritdoc />
    protected override string[] CalculateUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetDocumentBlueprintStartNodePaths(_entityService, _appCaches)
           ?? [];
}
