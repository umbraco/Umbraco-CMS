using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

/// <summary>
/// Serves as the base controller for handling operations related to data type trees in the Umbraco CMS Management API.
/// </summary>
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DataType}")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class DataTypeTreeControllerBase : FolderTreeControllerBase<DataTypeTreeItemResponseModel>
{
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeTreeControllerBase"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing Umbraco entities.</param>
    /// <param name="dataTypeService">Service for managing data types within Umbraco.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public DataTypeTreeControllerBase(IEntityService entityService, IDataTypeService dataTypeService)
        : this(
              entityService,
              StaticServiceProvider.Instance.GetRequiredService<FlagProviderCollection>(),
              dataTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeTreeControllerBase"/> class with the specified services.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the data type tree.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="dataTypeService">Service used for managing data types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public DataTypeTreeControllerBase(IEntityService entityService, FlagProviderCollection flagProviders, IDataTypeService dataTypeService)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IEntitySearchService>(),
            StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>(),
            dataTypeService)
    {
    }

    public DataTypeTreeControllerBase(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IDataTypeService dataTypeService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap) =>
        _dataTypeService = dataTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DataType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DataTypeContainer;

    protected override Ordering ItemOrdering
    {
        get
        {
            var ordering = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.NodeObjectTypeColumnName, Direction.Descending); // We need to override to change direction
            ordering.Next = Ordering.By(Infrastructure.Persistence.Dtos.NodeDto.TextColumnName);

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
