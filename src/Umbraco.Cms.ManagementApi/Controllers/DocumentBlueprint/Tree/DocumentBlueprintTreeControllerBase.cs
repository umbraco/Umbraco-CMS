using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.DocumentBlueprint.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DocumentBlueprint}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.DocumentBlueprint))]
public class DocumentBlueprintTreeControllerBase : EntityTreeControllerBase<DocumentBlueprintTreeItemViewModel>
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentBlueprintTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentBlueprint;

    protected override DocumentBlueprintTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var contentTypeAliases = entities
            .OfType<IDocumentEntitySlim>()
            .Select(entity => entity.ContentTypeAlias)
            .ToArray();

        var contentTypeIds = _contentTypeService.GetAllContentTypeIds(contentTypeAliases).ToArray();
        var contentTypeByAlias = _contentTypeService
            .GetAll(contentTypeIds)
            .ToDictionary(contentType => contentType.Alias);

        return entities.Select(entity =>
        {
            DocumentBlueprintTreeItemViewModel viewModel = base.MapTreeItemViewModel(parentKey, entity);
            viewModel.Icon = Constants.Icons.Blueprint;
            viewModel.HasChildren = false;

            if (entity is IDocumentEntitySlim documentEntitySlim
                && contentTypeByAlias.TryGetValue(documentEntitySlim.ContentTypeAlias, out IContentType? contentType))
            {
                viewModel.DocumentTypeKey = contentType.Key;
                viewModel.DocumentTypeAlias = contentType.Alias;
                viewModel.DocumentTypeName = contentType.Name;
            }

            return viewModel;
        }).ToArray();
    }
}
