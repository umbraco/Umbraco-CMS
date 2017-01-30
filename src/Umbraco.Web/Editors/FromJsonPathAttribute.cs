using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Used to bind a value from an inner json property
    /// </summary>
    /// <remarks>
    /// An example would be if you had json like:
    /// { ids: [1,2,3,4] }
    /// 
    /// And you had an action like: GetByIds(int[] ids, UmbracoEntityTypes type)
    /// 
    /// The ids array will not bind because the object being sent up is an object and not an array so the 
    /// normal json formatter will not figure this out. 
    /// 
    /// This would also let you bind sub levels of the JSON being sent up too if you wanted with any jsonpath
    /// </remarks>
    internal class FromJsonPathAttribute : ModelBinderAttribute
    {
        private readonly string _jsonPath;

        public FromJsonPathAttribute()
        {
        }

        public FromJsonPathAttribute(string jsonPath) : base(typeof(JsonPathBinder))
        {
            _jsonPath = jsonPath;
        }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            var config = parameter.Configuration;
            var binder = new JsonPathBinder(_jsonPath);
            var valueProviderFactories = GetValueProviderFactories(config);

            return new ModelBinderParameterBinding(parameter, binder, valueProviderFactories);
        }

        private class JsonPathBinder : IModelBinder
        {
            private readonly string _jsonPath;

            public JsonPathBinder(string jsonPath)
            {
                _jsonPath = jsonPath;
            }

            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                var requestContent = new HttpMessageContent(actionContext.Request);
                var strJson = requestContent.HttpRequestMessage.Content.ReadAsStringAsync().Result;
                var json = JsonConvert.DeserializeObject<JObject>(strJson);
                
                //if no explicit json path then use the model name
                var match = json.SelectToken(_jsonPath ?? bindingContext.ModelName);

                if (match == null)
                {
                    return false;
                }

                bindingContext.Model = match.ToObject(bindingContext.ModelType);

                return true;
            }
        }
        
        
    }
}