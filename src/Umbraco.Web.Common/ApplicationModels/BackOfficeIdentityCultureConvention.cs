using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Common.ApplicationModels
{
    public class BackOfficeIdentityCultureConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            action.Filters.Add(new BackOfficeCultureFilter());
        }
    }
}
