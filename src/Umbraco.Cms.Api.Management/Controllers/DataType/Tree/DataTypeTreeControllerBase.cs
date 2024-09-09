using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class DataTypeTreeControllerBase : FolderTreeControllerBase<DataTypeTreeItemResponseModel>
{
    private readonly IDataTypeService _dataTypeService;

    public DataTypeTreeControllerBase(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService) =>
        _dataTypeService = dataTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DataType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DataTypeContainer;

    protected override Ordering ItemOrdering
    {
        get
        {
            var ordering = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectType), Direction.Descending); // We need to override to change direction
            ordering.Next = Ordering.By(nameof(Infrastructure.Persistence.Dtos.NodeDto.Text));

            return ordering;
        }
    }

    protected override DataTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentId, IEntitySlim[] entities)
    {
        Dictionary<int, IDataType> dataTypes = entities.Any()
            ? _dataTypeService
                .GetAllAsync(entities.Select(entity => entity.Key).ToArray()).GetAwaiter().GetResult()
                .ToDictionary(contentType => contentType.Id)
            : new Dictionary<int, IDataType>();

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
