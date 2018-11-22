using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// A model binder to trim the string
    /// </summary>
    internal class TrimModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult?.AttemptedValue == null)
            {
                return false;
            }

            bindingContext.Model = (string.IsNullOrWhiteSpace(valueResult.AttemptedValue) ? valueResult.AttemptedValue : valueResult.AttemptedValue.Trim());
            return true;
        }
    }
}
