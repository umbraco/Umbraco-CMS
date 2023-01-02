using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataTypeViewModel>> Create(DataTypeCreateModel dataTypeCreateModel)
    {
        IDataType? created = _umbracoMapper.Map<IDataType>(dataTypeCreateModel);
        if (created == null)
        {
            return BadRequest("Could not map the POSTed model to a data type");
        }

        ProblemDetails? validationIssues = Save(created, _dataTypeService, _backOfficeSecurityAccessor);
        if (validationIssues != null)
        {
            return BadRequest(validationIssues);
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<DataTypeViewModel>(created)));
    }
}
