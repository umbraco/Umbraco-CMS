using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Template}/query")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessTemplates)]
public abstract class TemplateQueryControllerBase : TemplateControllerBase
{
    protected IEnumerable<TemplateQueryOperatorViewModel> GetOperators() => new TemplateQueryOperatorViewModel[]
    {
        new() { Operator = Operator.Equals, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.String } },
        new() { Operator = Operator.NotEquals, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.String } },
        new() { Operator = Operator.LessThan, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.DateTime } },
        new() { Operator = Operator.LessThanEqualTo, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.DateTime } },
        new() { Operator = Operator.GreaterThan, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.DateTime } },
        new() { Operator = Operator.GreaterThanEqualTo, ApplicableTypes = new[] { TemplateQueryPropertyType.Integer, TemplateQueryPropertyType.DateTime } },
        new() { Operator = Operator.Contains, ApplicableTypes = new[] { TemplateQueryPropertyType.String } },
        new() { Operator = Operator.NotContains, ApplicableTypes = new[] { TemplateQueryPropertyType.String } },
    };

    protected IEnumerable<TemplateQueryPropertyPresentationModel> GetProperties() => new TemplateQueryPropertyPresentationModel[]
    {
        new() { Alias = "Id", Type = TemplateQueryPropertyType.Integer },
        new() { Alias = "Name", Type = TemplateQueryPropertyType.String },
        new() { Alias = "CreateDate", Type = TemplateQueryPropertyType.DateTime },
        new() { Alias = "UpdateDate", Type = TemplateQueryPropertyType.DateTime }
    };
}
