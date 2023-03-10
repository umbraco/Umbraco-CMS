using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

public class CreateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDataTypeController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateDataTypeRequestModel createDataTypeRequestModel)
    {
        IDataType? created = _umbracoMapper.Map<IDataType>(createDataTypeRequestModel)!;
        Attempt<IDataType, DataTypeOperationStatus> result = await _dataTypeService.CreateAsync(created, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtAction<ByKeyDataTypeController>(controller => nameof(controller.ByKey), created.Key)
            : DataTypeOperationStatusResult(result.Status);
    }
}
