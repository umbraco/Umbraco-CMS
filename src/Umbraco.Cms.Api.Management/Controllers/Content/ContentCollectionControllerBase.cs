using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Content;

/// <summary>
/// Serves as a base controller for managing collections of content items, providing shared functionality for handling content collections and their variants.
/// </summary>
/// <typeparam name="TContent">The content entity type.</typeparam>
/// <typeparam name="TCollectionResponseModel">The response model type for the content collection.</typeparam>
/// <typeparam name="TValueResponseModelBase">The base type for value response models within the collection.</typeparam>
/// <typeparam name="TVariantResponseModel">The response model type for content variants.</typeparam>
public abstract class ContentCollectionControllerBase<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel> : ManagementApiControllerBase
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    private readonly IUmbracoMapper _mapper;
    private readonly FlagProviderCollection _flagProviders;

    protected ContentCollectionControllerBase(IUmbracoMapper mapper, FlagProviderCollection flagProvider)
    {
        _mapper = mapper;
        _flagProviders = flagProvider;
    }

    /// <summary>
    /// Creates a collection result from the provided collection response models and total number of items.
    /// </summary>
    protected IActionResult CollectionResult(List<TCollectionResponseModel> collectionResponseModels, long totalNumberOfItems)
    {
        var pageViewModel = new PagedViewModel<TCollectionResponseModel>
        {
            Items = collectionResponseModels,
            Total = totalNumberOfItems,
        };

        return Ok(pageViewModel);
    }

    protected IActionResult ContentCollectionOperationStatusResult(ContentCollectionOperationStatus status, string type) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentCollectionOperationStatus.CollectionNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("Collection data type could not be found")
                .WithDetail($"No collection data type was found for the corresponding {type} type. Ensure that the default and/or a custom collection data types exists")
                .Build()),
            ContentCollectionOperationStatus.ContentNotCollection => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle($"The {type} item is not configured as a collection")
                .WithDetail($"The specified {type} is not configured as a collection")
                .Build()),
            ContentCollectionOperationStatus.ContentNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle($"The specified {type} could not be found")
                .Build()),
            ContentCollectionOperationStatus.ContentTypeNotFound => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle($"The related {type} type could not be found")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotCollection => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Data type id does not belong to a collection")
                .WithDetail("The specified data type does not represent a collection")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotContentProperty => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle($"Data type id is not a {type} property")
                .WithDetail($"The specified data type is not part of the {type} properties")
                .Build()),
            ContentCollectionOperationStatus.DataTypeNotFound => new NotFoundObjectResult(problemDetailsBuilder
                .WithTitle("The specified collection data type could not be found")
                .Build()),
            ContentCollectionOperationStatus.DataTypeWithoutContentType => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle($"Missing {type} when specifying a collection data type")
                .WithDetail($"The specified collection data type needs to be used in conjunction with a {type} item.")
                .Build()),
            ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Missing properties in collection configuration")
                .WithDetail("No properties are configured to display in the collection configuration")
                .Build()),
            ContentCollectionOperationStatus.OrderByNotPartOfCollectionConfiguration => new BadRequestObjectResult(problemDetailsBuilder
                .WithTitle("Order by value is not a property on the configured collection")
                .WithDetail("The specified orderBy property is not part of the collection configuration")
                .Build()),
            _ => new ObjectResult("Unknown content collection operation status")
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            },
        });
}
