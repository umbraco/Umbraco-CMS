using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.Cms.Web.Common.ApplicationModels
{
    /// <summary>
    /// Applies the <see cref="UmbracoJsonModelBinder"/> body model binder to any parameter binding source of type <see cref="BindingSource.Body"/>
    /// </summary>
    /// <remarks>
    /// For this to work Microsoft's own <see cref="InferParameterBindingInfoConvention"/> convention must be executed before this one
    /// </remarks>
    public class UmbracoJsonModelBinderConvention : IActionModelConvention
    {
        /// <inheritdoc/>
        public void Apply(ActionModel action)
        {
            foreach (ParameterModel p in action.Parameters.Where(p => p.BindingInfo?.BindingSource == BindingSource.Body))
            {
                p.BindingInfo.BinderType = typeof(UmbracoJsonModelBinder);
            }
        }
    }
}
