using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DataType)]
[ApiExplorerSettings(GroupName = "Data Type")]
[ApiVersion("1.0")]
public abstract class DataTypeControllerBase : ManagementApiControllerBase
{
    protected static ProblemDetails? Save(IDataType dataType, IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        ValidationResult[] validationResults = dataTypeService.ValidateConfigurationData(dataType).ToArray();
        if (validationResults.Any())
        {
            return new ProblemDetailsBuilder()
                .WithTitle("Invalid data type configuration")
                .WithDetail(string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage)))
                .Build();
        }

        dataTypeService.Save(dataType, CurrentUserId(backOfficeSecurityAccessor));

        return null;
    }
}
