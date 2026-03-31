using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Filter;

/// <summary>
/// Controller responsible for handling operations related to filters on data types in the management API.
/// </summary>
[ApiVersion("1.0")]
public class FilterDataTypeFilterController : DataTypeFilterControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DataType.Filter.FilterDataTypeFilterController"/> class, responsible for filtering data types.
    /// </summary>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> used to manage data types.</param>
    /// <param name="mapper">The <see cref="IUmbracoMapper"/> used for mapping entities.</param>
    public FilterDataTypeFilterController(IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a paginated and filtered list of data types based on the specified criteria.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <param name="name">An optional filter to match data type names.</param>
    /// <param name="editorUiAlias">An optional filter to match the editor UI alias.</param>
    /// <param name="editorAlias">An optional filter to match the editor alias.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged collection of filtered data types.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a filtered collection of data types.")]
    [EndpointDescription("Filters data types based on the provided criteria with support for pagination.")]
    public async Task<IActionResult> Filter(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        string name = "",
        string? editorUiAlias = null,
        string? editorAlias = null)
    {
        PagedModel<IDataType> dataTypes = await _dataTypeService.FilterAsync(name, editorUiAlias, editorAlias, skip, take);
        List<DataTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes.Items);
        var viewModel = new PagedViewModel<DataTypeItemResponseModel>
        {
            Total = dataTypes.Total,
            Items = responseModels,
        };
        return Ok(viewModel);
    }
}
