using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Collection;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Collection}/{Constants.UdiEntityType.Document}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentCollectionControllerBase : ManagementApiControllerBase
{
    protected IActionResult CollectionOperationStatusResult(ContentCollectionOperationStatus status)
        => ContentCollectionOperationStatusResult(status, "document");

    internal static IActionResult ContentCollectionOperationStatusResult(ContentCollectionOperationStatus status, string type) =>
        status switch
        {
            ContentCollectionOperationStatus.CollectionNotFound => new NotFoundObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Collection data type could not be found")
                .WithDetail($"No collection data type was found for the corresponding {type} type. Ensure that the default and/or a custom collection data types exists")
                .Build()),
            ContentCollectionOperationStatus.ContentNotCollection => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle($"The {type} item is not configured as a collection")
                .WithDetail($"The specified {type} is not configured as a collection")
                .Build()),
            ContentCollectionOperationStatus.ContentNotFound => new NotFoundObjectResult(new ProblemDetailsBuilder()
                .WithTitle($"The specified {type} could not be found")
                .Build()),
            ContentCollectionOperationStatus.ContentTypeNotFound => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle($"The related {type} type could not be found")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotCollection => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Data type id does not belong to a collection")
                .WithDetail("The specified data type does not represent a collection")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotContentCollection => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Data type id does not represent the configured collection")
                .WithDetail($"The specified data type is not the configured collection for the given {type}")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotContentProperty => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle($"Data type id is not a {type} property")
                .WithDetail($"The specified data type is not part of the {type} properties")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotFound => new NotFoundObjectResult(new ProblemDetailsBuilder()
                .WithTitle("The specified collection data type could not be found")
                .Build()),
            ContentCollectionOperationStatus.DataTypeWithoutContentType => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle($"Missing {type} when specifying a collection data type")
                .WithDetail($"The specified collection data type needs to be used in conjunction with a {type} item.")
                .Build()),
            ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Missing properties in collection configuration")
                .WithDetail("No properties are configured to display in the collection configuration")
                .Build()),
            ContentCollectionOperationStatus.OrderByNotPartOfCollectionConfiguration => new BadRequestObjectResult(new ProblemDetailsBuilder()
                .WithTitle("Order by value is not a property on the configured collection")
                .WithDetail("The specified orderBy property is not part of the collection configuration")
                .Build()),
            _ => new ObjectResult("Unknown content collection operation status")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
}
