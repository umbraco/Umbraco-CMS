using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.ContentTypeEditing;

public class ContentTypeEditingService : ContentTypeEditingServiceBase<IContentType, IContentTypeService, ContentTypeCreateModel, ContentTypePropertyTypeModel, ContentTypePropertyContainerModel>, IContentTypeEditingService
{
    private readonly ITemplateService _templateService;
    private readonly IContentTypeService _contentTypeService;

    public ContentTypeEditingService(
        IContentTypeService contentTypeService,
        ITemplateService templateService,
        IDataTypeService dataTypeService,
        IEntityService entityService,
        IShortStringHelper shortStringHelper)
        : base(contentTypeService, contentTypeService, dataTypeService, entityService, shortStringHelper)
    {
        _contentTypeService = contentTypeService;
        _templateService = templateService;
    }

    public async Task<Attempt<IContentType?, ContentTypeOperationStatus>> CreateAsync(ContentTypeCreateModel model, Guid userKey)
    {
        Attempt<IContentType?, ContentTypeOperationStatus> result = await HandleCreateAsync(model);
        if (result.Success is false)
        {
            return result;
        }

        IContentType contentType = result.Result ?? throw new InvalidOperationException($"{nameof(HandleCreateAsync)} succeeded but did not yield any result");

        // update content type history clean-up
        contentType.HistoryCleanup ??= new HistoryCleanup();
        contentType.HistoryCleanup.PreventCleanup = model.Cleanup.PreventCleanup;
        contentType.HistoryCleanup.KeepAllVersionsNewerThanDays = model.Cleanup.KeepAllVersionsNewerThanDays;
        contentType.HistoryCleanup.KeepLatestVersionPerDayForDays = model.Cleanup.KeepLatestVersionPerDayForDays;

        // update allowed templates and assign default template
        ITemplate[] allowedTemplates = model.AllowedTemplateKeys
            .Select(async templateId => await _templateService.GetAsync(templateId))
            .Select(t => t.Result)
            .WhereNotNull()
            .ToArray();
        contentType.AllowedTemplates = allowedTemplates;
        // NOTE: incidentally this also covers removing the default template; when requestModel.DefaultTemplateId is null,
        //       contentType.SetDefaultTemplate() will be called with a null value, which will reset the default template.
        contentType.SetDefaultTemplate(allowedTemplates.FirstOrDefault(t => t.Key == model.DefaultTemplateKey));

        // save content type
        // FIXME: create and use an async get method here.
        // TODO: userKey => ID (or create async save with key)
        _contentTypeService.Save(contentType);

        return Attempt.SucceedWithStatus<IContentType?, ContentTypeOperationStatus>(ContentTypeOperationStatus.Success, contentType);
    }

    protected override Guid[] GetAvailableCompositionKeys(IContentTypeComposition? source, IContentTypeComposition[] allContentTypes, bool isElement)
        => _contentTypeService.GetAvailableCompositeContentTypes(source, allContentTypes, isElement: isElement)
            .Results
            .Where(x => x.Allowed)
            .Select(x => x.Composition.Key)
            .ToArray();

    protected override IContentType CreateContentType(IShortStringHelper shortStringHelper, int parentId)
        => new ContentType(shortStringHelper, parentId);

    protected override bool SupportsPublishing => true;

    protected override UmbracoObjectTypes ContentObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes ContainerObjectType => UmbracoObjectTypes.DocumentTypeContainer;
}
