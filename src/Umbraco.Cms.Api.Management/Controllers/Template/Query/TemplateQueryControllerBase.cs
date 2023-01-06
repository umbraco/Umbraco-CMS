using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Template.Query;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Query;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Template}/query")]
public abstract class TemplateQueryControllerBase : TemplateControllerBase
{
    protected IEnumerable<TemplateQueryOperatorViewModel> GetOperators() => new TemplateQueryOperatorViewModel[]
    {
        new() { Operator = Operator.Equals, AppliesTo = new[] { PropertyTypes.String } },
        new() { Operator = Operator.NotEquals, AppliesTo = new[] { PropertyTypes.String } },
        new() { Operator = Operator.LessThan, AppliesTo = new[] { PropertyTypes.DateTime } },
        new() { Operator = Operator.LessThanEqualTo, AppliesTo = new[] { PropertyTypes.DateTime } },
        new() { Operator = Operator.GreaterThan, AppliesTo = new[] { PropertyTypes.DateTime } },
        new() { Operator = Operator.GreaterThanEqualTo, AppliesTo = new[] { PropertyTypes.DateTime } },
        new() { Operator = Operator.Equals, AppliesTo = new[] { PropertyTypes.Integer } },
        new() { Operator = Operator.NotEquals, AppliesTo = new[] { PropertyTypes.Integer } },
        new() { Operator = Operator.Contains, AppliesTo = new[] { PropertyTypes.String } },
        new() { Operator = Operator.NotContains, AppliesTo = new[] { PropertyTypes.String } },
        new() { Operator = Operator.GreaterThan, AppliesTo = new[] { PropertyTypes.Integer } },
        new() { Operator = Operator.GreaterThanEqualTo, AppliesTo = new[] { PropertyTypes.Integer } },
        new() { Operator = Operator.LessThan, AppliesTo = new[] { PropertyTypes.Integer } },
        new() { Operator = Operator.LessThanEqualTo, AppliesTo = new[] { PropertyTypes.Integer } }
    };

    protected IEnumerable<TemplateQueryPropertyViewModel> GetProperties() => new TemplateQueryPropertyViewModel[]
    {
        new() { Alias = "Id", Type = PropertyTypes.Integer },
        new() { Alias = "Name", Type = PropertyTypes.String },
        new() { Alias = "CreateDate", Type = PropertyTypes.DateTime },
        new() { Alias = "UpdateDate", Type = PropertyTypes.DateTime }
    };

    protected static class PropertyTypes
    {
        public const string String = "string";

        public const string DateTime = "datetime";

        public const string Integer = "int";
    }
}
