using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

/// <summary>
/// API controller responsible for retrieving sibling element tree items.
/// </summary>
[ApiVersion("1.0")]
public class SiblingsElementTreeController : ElementTreeControllerBase
{
    /// <inheritdoc />
    [ActivatorUtilitiesConstructor]
    public SiblingsElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IElementStartNodeTreeFilterService treeFilterService,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, treeFilterService, elementPresentationFactory, elementPermissionFilterService)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public SiblingsElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IElementStartNodeTreeFilterService>(),
            elementPresentationFactory,
            elementPermissionFilterService)
    {
    }

    /// <summary>
    /// Gets a collection of sibling element tree items for the specified target.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the target element.</param>
    /// <param name="before">The number of sibling items to retrieve before the target.</param>
    /// <param name="after">The number of sibling items to retrieve after the target.</param>
    /// <param name="foldersOnly">Whether to return only folder items.</param>
    /// <returns>A subset collection of sibling element tree items.</returns>
    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(SubsetViewModel<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of element tree sibling items.")]
    [EndpointDescription("Gets a collection of element tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<ElementTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
