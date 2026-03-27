using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly FlagProviderCollection _flagProviderCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Factories.DocumentPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    /// <param name="documentUrlFactory">Factory for creating document URLs.</param>
    /// <param name="templateService">Service for managing templates.</param>
    /// <param name="publicAccessService">Service for controlling public access to documents.</param>
    /// <param name="timeProvider">Provider for time-related operations.</param>
    /// <param name="idKeyMap">Map for managing ID keys.</param>
    [Obsolete("Please use the controller with all parameters. Scheduled for removal in Umbraco 18")]
    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IDocumentUrlFactory documentUrlFactory,
        ITemplateService templateService,
        IPublicAccessService publicAccessService,
        TimeProvider timeProvider,
        IIdKeyMap idKeyMap)
        : this(
            umbracoMapper,
            documentUrlFactory,
            templateService,
            publicAccessService,
            timeProvider,
            idKeyMap,
            StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco models.</param>
    /// <param name="documentUrlFactory">Factory for generating URLs for documents.</param>
    /// <param name="templateService">Service for managing and retrieving templates.</param>
    /// <param name="publicAccessService">Service for handling public access and permissions.</param>
    /// <param name="timeProvider">Provider for obtaining the current time.</param>
    /// <param name="idKeyMap">Service for mapping between IDs and keys.</param>
    /// <param name="flagProviderCollection">Collection of providers for document flags.</param>
    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IDocumentUrlFactory documentUrlFactory,
        ITemplateService templateService,
        IPublicAccessService publicAccessService,
        TimeProvider timeProvider,
        IIdKeyMap idKeyMap,
        FlagProviderCollection flagProviderCollection)
    {
        _umbracoMapper = umbracoMapper;
        _documentUrlFactory = documentUrlFactory;
        _templateService = templateService;
        _publicAccessService = publicAccessService;
        _timeProvider = timeProvider;
        _idKeyMap = idKeyMap;
        _flagProviderCollection = flagProviderCollection;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="PublishedDocumentResponseModel"/> from the specified <see cref="IContent"/> instance.
    /// </summary>
    /// <param name="content">The content item from which to generate the published document response model.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result contains the generated <see cref="PublishedDocumentResponseModel"/>,
    /// including template reference information if available.
    /// </returns>
    public async Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content)
    {
        PublishedDocumentResponseModel responseModel = _umbracoMapper.Map<PublishedDocumentResponseModel>(content)!;

        Guid? templateKey = content.PublishTemplateId.HasValue
            ? _templateService.GetAsync(content.PublishTemplateId.Value).Result?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="DocumentResponseModel"/> from the specified <see cref="IContent"/> and <see cref="ContentScheduleCollection"/>.
    /// Maps the content and schedule to the response model, and if the content has an associated template, includes its reference in the result.
    /// </summary>
    /// <param name="content">The content item to map to the response model.</param>
    /// <param name="schedule">The collection of content schedules to include in the response model.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the constructed <see cref="DocumentResponseModel"/>, including template reference if applicable.</returns>
    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule)
    {
        DocumentResponseModel responseModel = _umbracoMapper.Map<DocumentResponseModel>(content)!;
        _umbracoMapper.Map(schedule, responseModel);

        Guid? templateKey = content.TemplateId.HasValue
            ? _templateService.GetAsync(content.TemplateId.Value).Result?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    /// <summary>
    /// Creates a <see cref="DocumentItemResponseModel"/> from the specified <see cref="IDocumentEntitySlim"/> entity.
    /// Populates the response model with the document's key properties, including parent reference, trashed status, protection status, document type, variants, and additional flags.
    /// </summary>
    /// <param name="entity">The document entity to create the response model from.</param>
    /// <returns>
    /// A <see cref="DocumentItemResponseModel"/> representing the document entity, with populated metadata and references.
    /// </returns>
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

        PopulateFlagsOnDocuments(responseModel);

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

    /// <summary>
    /// Generates a collection of <see cref="DocumentVariantItemResponseModel"/> objects representing each variant (culture) of the specified document entity.
    /// </summary>
    /// <param name="entity">The document entity for which to generate variant response models.</param>
    /// <returns>An <see cref="IEnumerable{DocumentVariantItemResponseModel}"/> containing a response model for each variant of the document.</returns>
    public IEnumerable<DocumentVariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false)
        {
            var model = new DocumentVariantItemResponseModel()
            {
                Name = entity.Name ?? string.Empty,
                State = DocumentVariantStateHelper.GetState(entity, null),
                Culture = null,
            };

            PopulateFlagsOnVariants(model);
            yield return model;
            yield break;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            var model = new DocumentVariantItemResponseModel()
            {
                Name = cultureNamePair.Value,
                Culture = cultureNamePair.Key,
                State = DocumentVariantStateHelper.GetState(entity, cultureNamePair.Key)
            };

            PopulateFlagsOnVariants(model);
            yield return model;
        }
    }

    /// <summary>
    /// Creates a <see cref="DocumentTypeReferenceResponseModel"/> from the given <see cref="IDocumentEntitySlim"/> entity.
    /// </summary>
    /// <param name="entity">The document entity to map.</param>
    /// <returns>A mapped <see cref="DocumentTypeReferenceResponseModel"/> instance.</returns>
    public DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity)
        => _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!;

    /// <summary>
    /// Creates a list of <see cref="Umbraco.Cms.Api.Management.Models.Content.CulturePublishScheduleModel"/> instances from the specified <see cref="Umbraco.Cms.Api.Management.Models.Content.PublishDocumentRequestModel"/>.
    /// Validates the publish and unpublish times for each culture's schedule, ensuring they are in the future and that unpublish times are after publish times.
    /// Returns an <see cref="Umbraco.Cms.Core.Models.Attempt"/> containing the resulting list and a <see cref="Umbraco.Cms.Api.Management.Models.Content.ContentPublishingOperationStatus"/> indicating the outcome of the validation.
    /// </summary>
    /// <param name="requestModel">The request model containing culture-specific publish schedules to process.</param>
    /// <returns>An <see cref="Umbraco.Cms.Core.Models.Attempt"/> with a list of <see cref="Umbraco.Cms.Api.Management.Models.Content.CulturePublishScheduleModel"/> and the validation status.</returns>
    public Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> CreateCulturePublishScheduleModels(PublishDocumentRequestModel requestModel)
    {
        var model = new List<CulturePublishScheduleModel>();

        foreach (CultureAndScheduleRequestModel cultureAndScheduleRequestModel in requestModel.PublishSchedules)
        {
            if (cultureAndScheduleRequestModel.Schedule is null)
            {
                model.Add(new CulturePublishScheduleModel
                {
                    Culture = cultureAndScheduleRequestModel.Culture
                              ?? Constants.System.InvariantCulture // API have `null` for invariant, but service layer has "*".
                });
                continue;
            }

            if (cultureAndScheduleRequestModel.Schedule.PublishTime is not null
                && cultureAndScheduleRequestModel.Schedule.PublishTime <= _timeProvider.GetUtcNow())
            {
                return Attempt.FailWithStatus(ContentPublishingOperationStatus.PublishTimeNeedsToBeInFuture, model);
            }

            if (cultureAndScheduleRequestModel.Schedule.UnpublishTime is not null
                && cultureAndScheduleRequestModel.Schedule.UnpublishTime <= _timeProvider.GetUtcNow())
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

    private void PopulateFlagsOnDocuments(DocumentItemResponseModel model)
    {
        foreach (IFlagProvider signProvider in _flagProviderCollection.Where(x => x.CanProvideFlags<DocumentItemResponseModel>()))
        {
            signProvider.PopulateFlagsAsync([model]).GetAwaiter().GetResult();
        }
    }

    private void PopulateFlagsOnVariants(DocumentVariantItemResponseModel model)
    {
        foreach (IFlagProvider signProvider in _flagProviderCollection.Where(x => x.CanProvideFlags<DocumentVariantItemResponseModel>()))
        {
            signProvider.PopulateFlagsAsync([model]).GetAwaiter().GetResult();
        }
    }
}
