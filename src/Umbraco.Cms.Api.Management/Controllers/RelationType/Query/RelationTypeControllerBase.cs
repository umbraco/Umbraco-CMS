﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.RelationType.Query;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.RelationType}")]
[ApiExplorerSettings(GroupName = "Relation Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessRelationTypes)]
public class RelationTypeControllerBase : ManagementApiControllerBase
{
        protected IActionResult RelationTypeOperationStatusResult(RelationTypeOperationStatus status) =>
        status switch
        {
            RelationTypeOperationStatus.InvalidId => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid id")
                .WithDetail("Can not assign an Id when creating a relation type")
                .Build()),
            RelationTypeOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the relation type operation.")
                .Build()),
            RelationTypeOperationStatus.KeyAlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Key already exists")
                .WithDetail("An entity with the given key already exists")
                .Build()),
            RelationTypeOperationStatus.NotFound => RelationTypeNotFound(),
            RelationTypeOperationStatus.InvalidChildObjectType => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid child object type")
                .WithDetail("The child object type is not allowed")
                .Build()),
            RelationTypeOperationStatus.InvalidParentObjectType => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid parent object type")
                .WithDetail("The parent object type is not allowed")
                .Build()),
        };

        protected IActionResult RelationTypeNotFound() => NotFound(new ProblemDetailsBuilder()
            .WithTitle("Relation type not found")
            .WithDetail("A relation type with the given key does not exist")
            .Build());

}
