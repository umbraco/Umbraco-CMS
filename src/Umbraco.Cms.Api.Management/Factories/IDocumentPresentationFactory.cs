using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Factory for creating document presentation (view) models from domain models.
/// </summary>
public interface IDocumentPresentationFactory
{
    /// <summary>
    /// Creates a published document response model from the specified content.
    /// </summary>
    /// <param name="content">The content to create the response model from.</param>
    /// <returns>A task that represents the asynchronous operation, containing the published document response model.</returns>
    Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content);

    /// <summary>
    /// Creates a document response model from the specified content and schedule.
    /// </summary>
    /// <param name="content">The content to create the response model from.</param>
    /// <param name="schedule">The content schedule collection.</param>
    /// <returns>A task that represents the asynchronous operation, containing the document response model.</returns>
    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule);

    /// <summary>
    /// Creates a document item response model from the specified entity.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <returns>The document item response model.</returns>
    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a search item response model for a document entity, including ancestor breadcrumbs.
    /// </summary>
    // TODO (V18): Remove default implementation.
    SearchDocumentItemResponseModel CreateSearchItemResponseModel(IDocumentEntitySlim entity, IEnumerable<SearchResultAncestorModel> ancestors)
    {
        DocumentItemResponseModel baseModel = CreateItemResponseModel(entity);
        return new SearchDocumentItemResponseModel
        {
            Id = baseModel.Id,
            IsTrashed = baseModel.IsTrashed,
            IsProtected = baseModel.IsProtected,
            Parent = baseModel.Parent,
            HasChildren = baseModel.HasChildren,
            DocumentType = baseModel.DocumentType,
            Variants = baseModel.Variants,
            Flags = baseModel.Flags,
            Ancestors = ancestors,
        };
    }

    /// <summary>
    /// Creates a blueprint item response model from the specified entity.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <returns>The document blueprint item response model.</returns>
    DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates variant item response models for the specified document entity.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <returns>The variant item response models.</returns>
    IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates a document type reference response model from the specified entity.
    /// </summary>
    /// <param name="entity">The document entity.</param>
    /// <returns>The document type reference response model.</returns>
    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity);

    /// <summary>
    /// Creates culture publish schedule models from the specified publish request.
    /// </summary>
    /// <param name="requestModel">The publish document request model containing schedule information.</param>
    /// <returns>An attempt containing the list of culture publish schedule models, or a failure status.</returns>
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
