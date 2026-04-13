using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating instances of element presentation models.
/// </summary>
public interface IElementPresentationFactory
{
    /// <summary>
    /// Creates a response model for the given element and its schedule.
    /// </summary>
    /// <param name="element">The element to create the response model for.</param>
    /// <param name="schedule">The schedule collection associated with the element.</param>
    /// <returns>An <see cref="ElementResponseModel"/> representing the element.</returns>
    ElementResponseModel CreateResponseModel(IElement element, ContentScheduleCollection schedule);

    /// <summary>
    /// Creates a response model for an element item based on the provided entity.
    /// </summary>
    /// <param name="entity">The element entity to create the response model from.</param>
    /// <returns>An <see cref="ElementItemResponseModel"/> representing the element item.</returns>
    Task<ElementItemResponseModel> CreateItemResponseModelAsync(IElementEntitySlim entity);

    /// <summary>
    /// Creates a collection of <see cref="ElementVariantItemResponseModel"/> instances representing the variants of the specified element entity.
    /// </summary>
    /// <param name="entity">The element entity to create variant response models for.</param>
    /// <returns>An enumerable of <see cref="ElementVariantItemResponseModel"/> representing the element variants.</returns>
    Task<IEnumerable<ElementVariantItemResponseModel>> CreateVariantsItemResponseModelsAsync(IElementEntitySlim entity);

    /// <summary>
    /// Creates a <see cref="DocumentTypeReferenceResponseModel"/> from the given <see cref="IElementEntitySlim"/> entity.
    /// </summary>
    /// <param name="entity">The element entity to create the response model from.</param>
    /// <returns>A <see cref="DocumentTypeReferenceResponseModel"/> representing the document type reference.</returns>
    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IElementEntitySlim entity);

    /// <summary>
    /// Creates a list of <see cref="CulturePublishScheduleModel"/> instances based on the provided <paramref name="requestModel"/>,
    /// validating each culture's publish and unpublish schedule for correctness (e.g., ensuring times are in the future and unpublish is after publish).
    /// </summary>
    /// <param name="requestModel">The request model containing culture-specific scheduling information for publishing elements.</param>
    /// <returns>An <see cref="Attempt{List{CulturePublishScheduleModel}, ContentPublishingOperationStatus}"/> containing the resulting list of culture publish schedules if validation succeeds, or a failure status with partial results if validation fails.</returns>
    Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> CreateCulturePublishScheduleModels(PublishElementRequestModel requestModel);
}
