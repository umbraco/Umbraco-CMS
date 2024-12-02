﻿using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentPresentationFactory : IDocumentPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDocumentUrlFactory _documentUrlFactory;
    private readonly ITemplateService _templateService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly TimeProvider _timeProvider;
    private readonly IIdKeyMap _idKeyMap;

    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IDocumentUrlFactory documentUrlFactory,
        ITemplateService templateService,
        IPublicAccessService publicAccessService,
        TimeProvider timeProvider,
        IIdKeyMap idKeyMap)
    {
        _umbracoMapper = umbracoMapper;
        _documentUrlFactory = documentUrlFactory;
        _templateService = templateService;
        _publicAccessService = publicAccessService;
        _timeProvider = timeProvider;
        _idKeyMap = idKeyMap;
    }

    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content)
    {
        DocumentResponseModel responseModel = _umbracoMapper.Map<DocumentResponseModel>(content)!;

        responseModel.Urls = await _documentUrlFactory.CreateUrlsAsync(content);

        Guid? templateKey = content.TemplateId.HasValue
            ? _templateService.GetAsync(content.TemplateId.Value).Result?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    public async Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content)
    {
        PublishedDocumentResponseModel responseModel = _umbracoMapper.Map<PublishedDocumentResponseModel>(content)!;

        responseModel.Urls = await _documentUrlFactory.CreateUrlsAsync(content);

        Guid? templateKey = content.PublishTemplateId.HasValue
            ? _templateService.GetAsync(content.PublishTemplateId.Value).Result?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    public DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.Document);

        var responseModel = new DocumentItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
        };

        responseModel.IsProtected = _publicAccessService.IsProtected(entity.Path);

        responseModel.DocumentType = _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

        responseModel.Variants = CreateVariantsItemResponseModels(entity);

        return responseModel;
    }

    public DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity)
    {
        var responseModel = new DocumentBlueprintItemResponseModel
        {
            Id = entity.Key,
            Name = entity.Name ?? string.Empty,
        };

        responseModel.DocumentType = _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

        return responseModel;
    }

    public IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false)
        {
            yield return new()
            {
                Name = entity.Name ?? string.Empty,
                State = DocumentVariantStateHelper.GetState(entity, null),
                Culture = null,
            };
            yield break;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            yield return new()
            {
                Name = cultureNamePair.Value,
                Culture = cultureNamePair.Key,
                State = DocumentVariantStateHelper.GetState(entity, cultureNamePair.Key)
            };
        }
    }

    public DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity)
        => _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

    public Attempt<CultureAndScheduleModel, ContentPublishingOperationStatus> CreateCultureAndScheduleModel(PublishDocumentRequestModel requestModel)
    {
        var contentScheduleCollection = new ContentScheduleCollection();
        var culturesToPublishImmediately = new HashSet<string>();
        foreach (CultureAndScheduleRequestModel cultureAndScheduleRequestModel in requestModel.PublishSchedules)
        {
            if (cultureAndScheduleRequestModel.Schedule is null || (cultureAndScheduleRequestModel.Schedule.PublishTime is null && cultureAndScheduleRequestModel.Schedule.UnpublishTime is null))
            {
                culturesToPublishImmediately.Add(cultureAndScheduleRequestModel.Culture ?? "*"); // API have `null` for invariant, but service layer has "*".
                continue;
            }

            if (cultureAndScheduleRequestModel.Schedule.PublishTime is not null)
            {
                if (cultureAndScheduleRequestModel.Schedule.PublishTime <= _timeProvider.GetUtcNow())
                {
                    return Attempt.FailWithStatus(ContentPublishingOperationStatus.PublishTimeNeedsToBeInFuture, new CultureAndScheduleModel()
                    {
                        Schedules = contentScheduleCollection,
                        CulturesToPublishImmediately = culturesToPublishImmediately,
                    });
                }

                contentScheduleCollection.Add(new ContentSchedule(
                    cultureAndScheduleRequestModel.Culture ?? "*",
                    cultureAndScheduleRequestModel.Schedule.PublishTime.Value.UtcDateTime,
                    ContentScheduleAction.Release));
            }
            if (cultureAndScheduleRequestModel.Schedule.UnpublishTime is not null)
            {
                if (cultureAndScheduleRequestModel.Schedule.UnpublishTime <= cultureAndScheduleRequestModel.Schedule.PublishTime)
                {
                    return Attempt.FailWithStatus(ContentPublishingOperationStatus.UnpublishTimeNeedsToBeAfterPublishTime, new CultureAndScheduleModel()
                    {
                        Schedules = contentScheduleCollection,
                        CulturesToPublishImmediately = culturesToPublishImmediately,
                    });
                }

                if (cultureAndScheduleRequestModel.Schedule.UnpublishTime <= _timeProvider.GetUtcNow())
                {
                    return Attempt.FailWithStatus(ContentPublishingOperationStatus.UpublishTimeNeedsToBeInFuture, new CultureAndScheduleModel()
                    {
                        Schedules = contentScheduleCollection,
                        CulturesToPublishImmediately = culturesToPublishImmediately,
                    });
                }

                contentScheduleCollection.Add(new ContentSchedule(
                    cultureAndScheduleRequestModel.Culture ?? "*",
                    cultureAndScheduleRequestModel.Schedule.UnpublishTime.Value.UtcDateTime,
                    ContentScheduleAction.Expire));
            }
        }
        return Attempt.SucceedWithStatus(ContentPublishingOperationStatus.Success, new CultureAndScheduleModel()
        {
            Schedules = contentScheduleCollection,
            CulturesToPublishImmediately = culturesToPublishImmediately,
        });
    }
}
