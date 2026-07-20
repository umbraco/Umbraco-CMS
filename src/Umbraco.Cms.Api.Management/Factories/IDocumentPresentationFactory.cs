using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for creating instances of document presentation models.
/// </summary>
public interface IDocumentPresentationFactory
{
    /// <summary>
    /// Creates a published document response model asynchronously from the given content.
    /// </summary>
    /// <param name="content">The content to create the published response model from.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the published document response model.</returns>
    Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content);

    /// <summary>
    /// Creates a response model for the given content and its schedule.
    /// </summary>
    /// <param name="content">The content to create the response model for.</param>
    /// <param name="schedule">The schedule collection associated with the content.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the document response model.</returns>
    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule);
    /// <summary>
    /// Creates a response model for a document item based on the provided entity.
    /// </summary>
    /// <param name="entity">The document entity to create the response model from.</param>
    /// <returns>A <see cref="DocumentItemResponseModel"/> representing the document item.</returns>
    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a <see cref="DocumentBlueprintItemResponseModel"/> from the specified document entity.
    /// </summary>
    /// <param name="entity">The slim document entity to convert.</param>
    /// <returns>A blueprint item response model representing the document.</returns>
    DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a collection of <see cref="DocumentVariantItemResponseModel"/> instances representing the variants of the specified document entity.
    /// </summary>
    /// <param name="entity">The document entity to create variant response models for.</param>
    /// <returns>An enumerable of <see cref="DocumentVariantItemResponseModel"/> representing the document variants.</returns>
    IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a <see cref="DocumentTypeReferenceResponseModel"/> from the given <see cref="IDocumentEntitySlim"/> entity.
    /// </summary>
    /// <param name="entity">The document entity slim instance to create the response model from.</param>
    /// <returns>A <see cref="DocumentTypeReferenceResponseModel"/> representing the document type reference.</returns>
    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a list of <see cref="CulturePublishScheduleModel"/> instances based on the provided <paramref name="requestModel"/>,
    /// validating each culture's publish and unpublish schedule for correctness (e.g., ensuring times are in the future and unpublish is after publish).
    /// </summary>
    /// <param name="requestModel">The request model containing culture-specific scheduling information for publishing documents.</param>
    /// <returns>An <see cref="Attempt{List{CulturePublishScheduleModel}, ContentPublishingOperationStatus}"/> containing the resulting list of culture publish schedules if validation succeeds, or a failure status with partial results if validation fails.</returns>
    Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> CreateCulturePublishScheduleModels(
        PublishDocumentRequestModel requestModel)
    {
        // todo remove default implementation when obsolete method is removed
        var model = new List<CulturePublishScheduleModel>();

        foreach (CultureAndScheduleRequestModel cultureAndScheduleRequestModel in requestModel.PublishSchedules)
        {
            if (cultureAndScheduleRequestModel.Schedule is null)
            {
                model.Add(new CulturePublishScheduleModel
                {
                    Culture = cultureAndScheduleRequestModel.Culture
                              ?? Constants.System.InvariantCulture
                });
                continue;
            }

            if (cultureAndScheduleRequestModel.Schedule.PublishTime is not null
                && cultureAndScheduleRequestModel.Schedule.PublishTime <= StaticServiceProvider.Instance.GetRequiredService<TimeProvider>().GetUtcNow())
            {
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.PublishTimeNeedsToBeInFuture, model);
            }

            if (cultureAndScheduleRequestModel.Schedule.UnpublishTime is not null
                && cultureAndScheduleRequestModel.Schedule.UnpublishTime <= StaticServiceProvider.Instance.GetRequiredService<TimeProvider>().GetUtcNow())
            {
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.UpublishTimeNeedsToBeInFuture, model);
            }

            if (cultureAndScheduleRequestModel.Schedule.UnpublishTime <= cultureAndScheduleRequestModel.Schedule.PublishTime)
            {
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.UnpublishTimeNeedsToBeAfterPublishTime, model);
            }

            model.Add(new CulturePublishScheduleModel
            {
                Culture = cultureAndScheduleRequestModel.Culture,
                Schedule = new ContentScheduleModel
                {
                    PublishDate = cultureAndScheduleRequestModel.Schedule.PublishTime,
                    UnpublishDate = cultureAndScheduleRequestModel.Schedule.UnpublishTime,
                },
            });
        }

        return Attempt.SucceedWithStatus(ContentPublishingOperationStatus.Success, model);
    }
}
