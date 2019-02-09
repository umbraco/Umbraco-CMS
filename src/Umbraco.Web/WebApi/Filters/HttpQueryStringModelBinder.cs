using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Allows an Action to execute with an arbitrary number of QueryStrings
    /// </summary>
    /// <remarks>
    /// Just like you can POST an arbitrary number of parameters to an Action, you can't GET an arbitrary number
    /// but this will allow you to do it
    /// </remarks>
    public sealed class HttpQueryStringModelBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            //get the query strings from the request properties
            if (actionContext.Request.Properties.ContainsKey("MS_QueryNameValuePairs"))
            {
                if (actionContext.Request.Properties["MS_QueryNameValuePairs"] is IEnumerable<KeyValuePair<string, string>> queryStrings)
                {
                    var queryStringKeys = queryStrings.Select(kvp => kvp.Key).ToArray();
                    var additionalParameters = new Dictionary<string, string>();
                    if(queryStringKeys.Contains("culture") == false) {
                        additionalParameters["culture"] = actionContext.Request.ClientCulture();
                    }

                    var formData = new FormDataCollection(queryStrings.Union(additionalParameters));
                    bindingContext.Model = formData;
                    return true;
                }
            }
            return false;
        }
    }
}
