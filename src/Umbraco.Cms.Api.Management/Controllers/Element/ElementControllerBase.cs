using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Serves as the base controller for implementing element management operations within the Umbraco CMS Management API.
/// Provides shared functionality for derived element controllers.
/// </summary>
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Element)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Element))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessElements)]
public class ElementControllerBase : ContentControllerBase
{
    protected override string EntityName => "element";

    protected IActionResult ElementEditingOperationStatusResult<TContentModelBase>(
        ContentEditingOperationStatus status,
        TContentModelBase requestModel,
        ContentValidationResult validationResult)
        where TContentModelBase : ContentModelBase<ElementValueModel, ElementVariantRequestModel>
        => ContentEditingOperationStatusResult<TContentModelBase, ElementValueModel, ElementVariantRequestModel>(status, requestModel, validationResult);

    /// <summary>
    /// Maps the combined status of a save-and-publish operation onto a result, using the editing or the publishing
    /// mapper depending on which part of the operation failed.
    /// </summary>
    /// <param name="status">The combined status of the save and the publish.</param>
    /// <param name="invalidPropertyAliases">The aliases of the properties that failed validation when publishing, if any.</param>
    /// <returns>The problem details for whichever part of the operation failed.</returns>
    protected IActionResult ElementEditingAndPublishingOperationStatusResult(
        ContentEditingAndPublishingStatus status,
        IEnumerable<string>? invalidPropertyAliases = null)
    {
        if (status.ContentEditingOperationStatus is not ContentEditingOperationStatus.Success)
        {
            return ContentEditingOperationStatusResult(status.ContentEditingOperationStatus);
        }

        if (status.ContentPublishingOperationStatus is { } publishingStatus
            && publishingStatus is not ContentPublishingOperationStatus.Success)
        {
            return ElementPublishingOperationStatusResult(publishingStatus, invalidPropertyAliases);
        }

        throw new ArgumentException(
            "The operation did not fail, so there are no problem details to report: the save reported "
            + $"'{status.ContentEditingOperationStatus}' and the publish reported "
            + $"'{status.ContentPublishingOperationStatus?.ToString() ?? "not attempted"}'. "
            + "Please handle a successful status explicitly in the controllers.",
            nameof(status));
    }

    protected IActionResult ElementPublishingOperationStatusResult(
        ContentPublishingOperationStatus status,
        IEnumerable<string>? invalidPropertyAliases = null,
        IEnumerable<ContentPublishingBranchItemResult>? failedBranchItems = null)
        => ContentPublishingOperationStatusResult(status, invalidPropertyAliases, failedBranchItems);
}
