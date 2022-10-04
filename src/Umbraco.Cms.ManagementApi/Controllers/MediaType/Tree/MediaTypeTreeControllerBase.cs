using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.MediaType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.MediaType}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.MediaType))]
public class MediaTypeTreeControllerBase : FolderTreeControllerBase<FolderTreeItemViewModel>
{
    private readonly IMediaTypeService _mediaTypeService;

    public MediaTypeTreeControllerBase(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService) =>
        _mediaTypeService = mediaTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.MediaType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.MediaTypeContainer;

    protected override FolderTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var mediaTypes = _mediaTypeService
            .GetAll(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            FolderTreeItemViewModel viewModel = MapTreeItemViewModel(parentKey, entity);
            if (mediaTypes.TryGetValue(entity.Id, out IMediaType? mediaType))
            {
                viewModel.Icon = mediaType.Icon ?? viewModel.Icon;
            }

            return viewModel;
        }).ToArray();
    }
}
