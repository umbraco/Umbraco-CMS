using Umbraco.Cms.Api.Management.Services.Flags;
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

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc cref="IDocumentPresentationFactory" />
internal sealed class DocumentPresentationFactory
    : PublishableContentPresentationFactoryBase<IDocumentEntitySlim, DocumentVariantItemResponseModel>,
      IDocumentPresentationFactory
{
    private readonly ITemplateService _templateService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPresentationFactory"/> class.
    /// </summary>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco models.</param>
    /// <param name="templateService">Service for managing and retrieving templates.</param>
    /// <param name="publicAccessService">Service for handling public access and permissions.</param>
    /// <param name="timeProvider">Provider for obtaining the current time.</param>
    /// <param name="idKeyMap">Service for mapping between IDs and keys.</param>
    /// <param name="flagProviderCollection">Collection of providers for document flags.</param>
    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        ITemplateService templateService,
        IPublicAccessService publicAccessService,
        TimeProvider timeProvider,
        IIdKeyMap idKeyMap,
        FlagProviderCollection flagProviderCollection)
        : base(umbracoMapper, flagProviderCollection, timeProvider)
    {
        _templateService = templateService;
        _publicAccessService = publicAccessService;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc/>
    public async Task<PublishedDocumentResponseModel> CreatePublishedResponseModelAsync(IContent content)
    {
        PublishedDocumentResponseModel responseModel = UmbracoMapper.Map<PublishedDocumentResponseModel>(content)!;

        Guid? templateKey = content.PublishTemplateId.HasValue
            ? (await _templateService.GetAsync(content.PublishTemplateId.Value))?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    /// <inheritdoc/>
    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content, ContentScheduleCollection schedule)
    {
        DocumentResponseModel responseModel = UmbracoMapper.Map<DocumentResponseModel>(content)!;
        UmbracoMapper.Map(schedule, responseModel);

        Guid? templateKey = content.TemplateId.HasValue
            ? (await _templateService.GetAsync(content.TemplateId.Value))?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    /// <inheritdoc/>
    [Obsolete("Use CreateItemResponseModelAsync instead. Scheduled for removal in Umbraco 19.")]
    public DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity)
        => CreateItemResponseModelAsync(entity).GetAwaiter().GetResult();

    /// <inheritdoc/>
    public async Task<DocumentItemResponseModel> CreateItemResponseModelAsync(IDocumentEntitySlim entity)
    {
        Attempt<Guid> parentKeyAttempt = _idKeyMap.GetKeyForId(entity.ParentId, UmbracoObjectTypes.Document);

        var responseModel = new DocumentItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed,
            Parent = parentKeyAttempt.Success ? new ReferenceByIdModel { Id = parentKeyAttempt.Result } : null,
            HasChildren = entity.HasChildren,
            IsProtected = _publicAccessService.IsProtected(entity.Path),
            DocumentType = CreateDocumentTypeReferenceResponseModel(entity),
            Variants = await CreateVariantsItemResponseModelsAsync(entity),
        };

        await PopulateFlagsAsync(responseModel);

        return responseModel;
    }

    /// <inheritdoc/>
    public DocumentBlueprintItemResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity)
    {
        var responseModel = new DocumentBlueprintItemResponseModel
        {
            Id = entity.Key,
            Name = entity.Name ?? string.Empty,
            DocumentType = UmbracoMapper.Map<DocumentTypeReferenceResponseModel>(entity)!,
        };

        return responseModel;
    }

    /// <inheritdoc/>
    public Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus>
        CreateCulturePublishScheduleModels(PublishDocumentRequestModel requestModel)
        => CreateCulturePublishScheduleModels(requestModel.PublishSchedules);

    /// <inheritdoc/>
    protected override DocumentVariantItemResponseModel CreateVariantItemResponseModel(
        string name,
        DocumentVariantState state,
        string? culture)
        => new() { Name = name, State = state, Culture = culture };
}
