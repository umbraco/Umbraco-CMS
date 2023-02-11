using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.DataType.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DataType}/tree")]
[OpenApiTag(nameof(Constants.UdiEntityType.DataType))]
public class DataTypeTreeControllerBase : FolderTreeControllerBase<FolderTreeItemViewModel>
{
    private readonly IDataTypeService _dataTypeService;

    public DataTypeTreeControllerBase(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService) =>
        _dataTypeService = dataTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DataType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DataTypeContainer;

    protected override FolderTreeItemViewModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var dataTypes = _dataTypeService
            .GetAll(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            FolderTreeItemViewModel viewModel = MapTreeItemViewModel(parentKey, entity);
            if (dataTypes.TryGetValue(entity.Id, out IDataType? dataType))
            {
                viewModel.Icon = dataType.Editor?.Icon ?? viewModel.Icon;
            }

            return viewModel;
        }).ToArray();
    }
}
