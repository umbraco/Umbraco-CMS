using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

public class UpdateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateDataTypeController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataTypeViewModel>> Update(Guid key, DataTypeUpdateModel dataTypeViewModel)
    {
        IDataType? current = _dataTypeService.GetDataType(key);
        if (current == null)
        {
            return NotFound();
        }

        IDataType updated = _umbracoMapper.Map(dataTypeViewModel, current);

        ProblemDetails? validationIssues = Save(updated, _dataTypeService, _backOfficeSecurityAccessor);
        if (validationIssues != null)
        {
            return BadRequest(validationIssues);
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<DataTypeViewModel>(updated)));
    }
}
