using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Web.Common.ModelBinding;
using System.Linq;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Actions;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.Common.ApplicationModels
{
    /// <summary>
    /// Applies the <see cref="UmbracoJsonModelBinder"/> body model binder to any parameter binding source of type <see cref="BindingSource.Body"/>
    /// </summary>
    /// <remarks>
    /// For this to work Microsoft's own <see cref="InferParameterBindingInfoConvention"/> convention must be executed before this one
    /// </remarks>
    public class UmbracoJsonModelBinderConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            foreach (var p in action.Parameters.Where(p => p.BindingInfo?.BindingSource == BindingSource.Body))
            {
                p.BindingInfo.BinderType = typeof(UmbracoJsonModelBinder);
            }
        }
    }

    
}
