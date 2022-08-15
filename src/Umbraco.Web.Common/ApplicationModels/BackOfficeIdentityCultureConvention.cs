using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Cms.Web.Common.Filters;

namespace Umbraco.Cms.Web.Common.ApplicationModels;

// TODO: This should just exist in the back office project
public class BackOfficeIdentityCultureConvention : IActionModelConvention
{
    /// <inheritdoc />
    public void Apply(ActionModel action) => action.Filters.Add(new BackOfficeCultureFilter());
}
