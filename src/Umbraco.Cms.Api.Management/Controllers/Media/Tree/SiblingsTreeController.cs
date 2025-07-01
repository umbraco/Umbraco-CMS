using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

public class SiblingsTreeController : MediaTreeControllerBase
{
    public SiblingsTreeController(IEntityService entityService, IUserStartNodeEntitiesService userStartNodeEntitiesService, IDataTypeService dataTypeService, AppCaches appCaches, IBackOfficeSecurityAccessor backofficeSecurityAccessor, IMediaPresentationFactory mediaPresentationFactory) : base(entityService, userStartNodeEntitiesService, dataTypeService, appCaches, backofficeSecurityAccessor, mediaPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<MediaTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after)
        => GetSiblings(target, before, after);
}
