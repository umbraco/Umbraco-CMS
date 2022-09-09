using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.DocumentType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DocumentType}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.DocumentType))]
public class DocumentTypeTreeControllerBase : FolderTreeControllerBase<DocumentTypeTreeItemViewModel>
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentTypeTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    protected override DocumentTypeTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var contentTypes = _contentTypeService
            .GetAll(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            DocumentTypeTreeItemViewModel viewModel = MapTreeItemViewModel(parentKey, entity);
            if (contentTypes.TryGetValue(entity.Id, out IContentType? contentType))
            {
                viewModel.Icon = contentType.Icon ?? viewModel.Icon;
                viewModel.IsElement = contentType.IsElement;
            }

            return viewModel;
        }).ToArray();
    }
}
