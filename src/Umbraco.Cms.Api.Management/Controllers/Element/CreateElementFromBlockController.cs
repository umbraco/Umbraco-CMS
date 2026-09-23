using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for creating an Element Library element from a block held in a content item's
/// property, reproducing the cultures the block is currently live in.
/// </summary>
/// <remarks>
/// The owner can be a document, a media item, a member or another element: block editors carry no restriction
/// on which tree they are used in, and a Library element holds block properties like any other content.
/// </remarks>
[ApiVersion("1.0")]
public class CreateElementFromBlockController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IEntityService _entityService;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateElementFromBlockController"/> class.
    /// </summary>
    public CreateElementFromBlockController(
        IAuthorizationService authorizationService,
        IEntityService entityService,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _authorizationService = authorizationService;
        _entityService = entityService;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a Library element from a block held in a content item's property.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="requestModel">Which block to take, and where to put the element.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpPost("from-block")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a Library element from a block held in a content item's property.")]
    [EndpointDescription(
        "Creates a new Library element from a nested element currently held in a block property on a document, " +
        "media item, member or another element, publishing the cultures it is currently live in as far as " +
        "validation allows.")]
    public async Task<IActionResult> CreateFromBlock(
        CancellationToken cancellationToken,
        CreateElementFromBlockRequestModel requestModel)
    {
        IEntitySlim? owner = _entityService.Get(requestModel.Owner.Id);
        if (owner is null)
        {
            return ElementCreateFromBlockOperationStatusResult(ElementCreateFromBlockOperationStatus.OwnerNotFound);
        }

        UmbracoObjectTypes ownerObjectType = Core.Models.ObjectTypes.GetUmbracoObjectType(owner.NodeObjectType);
        if (await AuthorizeOwnerAsync(ownerObjectType, requestModel.Owner.Id) is { Succeeded: false }
            || await AuthorizeCreateInLibraryAsync(requestModel.Parent?.Id) is { Succeeded: false })
        {
            return Forbidden();
        }

        Attempt<IElement?, ElementCreateFromBlockOperationStatus> result = await _elementEditingService.CreateFromBlockAsync(
            MapCreateModel(requestModel),
            allowPublish: await AuthorizePublishInLibraryAsync(requestModel.Parent?.Id) is { Succeeded: true },
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyElementController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : ElementCreateFromBlockOperationStatusResult(result.Status);
    }

    /// <summary>
    /// Authorizes the current user against the item that owns the block property.
    /// </summary>
    /// <param name="ownerObjectType">The object type the owner key resolved to.</param>
    /// <param name="ownerKey">The unique identifier of the owner.</param>
    /// <returns><c>false</c> for an owner of a type that cannot hold a block property, as for any other refusal.</returns>
    /// <remarks>
    /// The editor is editing the owner, so an update permission on it is required. Cultures are deliberately
    /// not checked: the values taken are the owner's already-public ones, not the editor's own edits.
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizeOwnerAsync(UmbracoObjectTypes ownerObjectType, Guid ownerKey)
    {
        switch (ownerObjectType)
        {
            case UmbracoObjectTypes.Document:
            {
                AuthorizationResult sectionResult = await _authorizationService.AuthorizeAsync(User, AuthorizationPolicies.TreeAccessDocuments);
                if (sectionResult.Succeeded is false)
                {
                    return sectionResult;
                }

                return await _authorizationService.AuthorizeResourceAsync(
                        User,
                        ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, ownerKey),
                        AuthorizationPolicies.ContentPermissionByResource);
            }

            case UmbracoObjectTypes.Media:
            {
                AuthorizationResult sectionResult = await _authorizationService.AuthorizeAsync(User, AuthorizationPolicies.SectionAccessMedia);
                if (sectionResult.Succeeded is false)
                {
                    return sectionResult;
                }

                return await _authorizationService.AuthorizeResourceAsync(
                        User,
                        MediaPermissionResource.WithKeys(ownerKey),
                        AuthorizationPolicies.MediaPermissionByResource);
            }

            case UmbracoObjectTypes.Member:
                return await _authorizationService.AuthorizeAsync(User, AuthorizationPolicies.SectionAccessMembers);

            case UmbracoObjectTypes.Element:
                // Controller already gates the route family on Library access, so only the per-item check needed.
                return await _authorizationService.AuthorizeResourceAsync(
                    User,
                    ElementPermissionResource.WithKeys(ActionElementUpdate.ActionLetter, ownerKey),
                    AuthorizationPolicies.ElementPermissionByResource);

            default:
                return AuthorizationResult.Failed();
        }
    }

    /// <summary>
    /// Authorizes the current user to create the new element in the target Library folder.
    /// </summary>
    /// <remarks>
    /// The target is always a Library folder. <c>ElementPermissionResource</c> resolves keys against the Element
    /// object type only, so it silently passes for a folder key; the container resource is the one that actually
    /// enforces action letters and start nodes against a folder.
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizeCreateInLibraryAsync(Guid? parentKey)
        => await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementContainerPermissionResource.WithKeys(ActionElementNew.ActionLetter, parentKey),
            AuthorizationPolicies.ElementFolderPermissionByResource);

    /// <summary>
    /// Determines whether the current user may publish in the target Library folder.
    /// </summary>
    /// <remarks>
    /// Unlike the create permission this is not a gate: the result is passed to the service as data, so the
    /// element is created either way. Reproducing the block's live state is best effort, and lacking the grant
    /// is one more reason it may not happen, alongside values that fail validation.
    /// <para>
    /// <c>CulturesToCheck</c> is deliberately omitted, unlike every other publish endpoint. The published version
    /// is built from the owner's own live property value (see <see cref="IElementEditingService" />), so nothing
    /// can become public that the editor could not already see.
    /// </para>
    /// </remarks>
    private async Task<AuthorizationResult> AuthorizePublishInLibraryAsync(Guid? parentKey)
        => await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementContainerPermissionResource.WithKeys(ActionElementPublish.ActionLetter, parentKey),
            AuthorizationPolicies.ElementFolderPermissionByResource);

    private static CreateElementFromBlockModel MapCreateModel(CreateElementFromBlockRequestModel requestModel)
        => new()
        {
            Key = requestModel.Id,
            OwnerKey = requestModel.Owner.Id,
            BlockKey = requestModel.Block.Id,
            ParentKey = requestModel.Parent?.Id,
            Name = requestModel.Name,
        };

    private IActionResult ElementCreateFromBlockOperationStatusResult(ElementCreateFromBlockOperationStatus status)
        => OperationStatusResult(
            status,
            problemDetailsBuilder => status switch
            {
                ElementCreateFromBlockOperationStatus.OwnerNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The owner could not be found")
                    .Build()),
                ElementCreateFromBlockOperationStatus.OwnerTypeNotSupported => BadRequest(problemDetailsBuilder
                    .WithTitle("Owner type not supported")
                    .WithDetail("Only documents, media items, members and elements can hold a block property to create from.")
                    .Build()),
                ElementCreateFromBlockOperationStatus.BlockNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The specified block could not be found")
                    .WithDetail("The owner's stored property value holds no block with that key. An unsaved block has nothing to read; save the owner first.")
                    .Build()),
                ElementCreateFromBlockOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The element content type could not be found")
                    .Build()),
                ElementCreateFromBlockOperationStatus.NotAllowed => BadRequest(problemDetailsBuilder
                    .WithTitle("Not allowed")
                    .WithDetail("The element's content type is not allowed in the Library.")
                    .Build()),
                ElementCreateFromBlockOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("The target Library folder could not be found")
                    .Build()),
                ElementCreateFromBlockOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                    .WithTitle("Cancelled by notification")
                    .WithDetail("The operation was cancelled by a notification handler.")
                    .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown element create from block operation status."),
            });
}
