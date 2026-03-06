using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// User start node tree filter service for document (content) trees.
/// Resolves the current user's content start nodes.
/// </summary>
internal sealed class DocumentStartNodeTreeFilterService : UserStartNodeTreeFilterService, IDocumentStartNodeTreeFilterService
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    public DocumentStartNodeTreeFilterService(
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
    protected override UmbracoObjectTypes[] TreeObjectTypes => [UmbracoObjectTypes.Document];

    /// <inheritdoc />
    protected override int[] CalculateUserStartNodeIds()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .CalculateContentStartNodeIds(_entityService, _appCaches)
           ?? Array.Empty<int>();

    /// <inheritdoc />
    protected override string[] CalculateUserStartNodePaths()
        => _backOfficeSecurityAccessor
               .BackOfficeSecurity?
               .CurrentUser?
               .GetContentStartNodePaths(_entityService, _appCaches)
           ?? Array.Empty<string>();
}
