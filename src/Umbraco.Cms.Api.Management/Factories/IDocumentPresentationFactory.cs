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

public interface IDocumentPresentationFactory
{
    Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content);

    Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule);

    [Obsolete("Use CreateItemResponseModelAsync instead. Scheduled for removal in Umbraco 19.")]
    DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity);

    DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity);

    [Obsolete("Use CreateVariantsItemResponseModelsAsync instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity);

    DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity);

    Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> CreateCulturePublishScheduleModels(
        PublishDocumentRequestModel requestModel)
    {
        // todo remove default implementation when obsolete method is removed
        var model = new List<CulturePublishScheduleModel>();

        foreach (CultureAndScheduleRequestModel cultureAndScheduleRequestModel in requestModel.PublishSchedules)
        {
            if (cultureAndScheduleRequestModel.Schedule is null)
            {
                model.Add(
                    new CulturePublishScheduleModel
                    {
                        Culture = cultureAndScheduleRequestModel.Culture ?? Constants.System.InvariantCulture,
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

            model.Add(
                new CulturePublishScheduleModel
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

    // TODO (V19): Remove the default implementation when CreateItemResponseModel is removed.
    Task<DocumentItemResponseModel> CreateItemResponseModelAsync(IDocumentEntitySlim entity)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(CreateItemResponseModel(entity));
#pragma warning restore CS0618 // Type or member is obsolete

    // TODO (V19): Remove the default implementation when CreateVariantsItemResponseModels is removed.
    Task<IEnumerable<DocumentVariantItemResponseModel>> CreateVariantsItemResponseModelsAsync(IDocumentEntitySlim entity)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(CreateVariantsItemResponseModels(entity));
#pragma warning restore CS0618 // Type or member is obsolete
}
