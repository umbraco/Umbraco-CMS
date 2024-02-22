using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDataTypes)]
public class DataTypeTreeControllerBase : FolderTreeControllerBase<DataTypeTreeItemResponseModel>
{
    private readonly IDataTypeService _dataTypeService;

    public DataTypeTreeControllerBase(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService) =>
        _dataTypeService = dataTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DataType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DataTypeContainer;

    protected override DataTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentId, IEntitySlim[] entities)
    {
        var dataTypes = _dataTypeService
            .GetAllAsync(entities.Select(entity => entity.Key).ToArray()).GetAwaiter().GetResult()
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            DataTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentId, entity);
            if (dataTypes.TryGetValue(entity.Id, out IDataType? dataType))
            {
                responseModel.EditorUiAlias = dataType.EditorUiAlias;
                responseModel.IsDeletable = dataType.IsDeletableDataType();
            }

            return responseModel;
        }).ToArray();
    }
}
