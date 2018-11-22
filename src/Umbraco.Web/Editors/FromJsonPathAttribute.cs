using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

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
        private readonly FromUriAttribute _fromUriAttribute = new FromUriAttribute();

        public FromJsonPathAttribute()
        {
        }

        public FromJsonPathAttribute(string jsonPath) : base(typeof(JsonPathBinder))
        {
            _jsonPath = jsonPath;
        }

        public override IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            return _fromUriAttribute.GetValueProviderFactories(configuration);
        }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            var config = parameter.Configuration;
            //get the default binder, we'll use that if it's a GET or if the body is empty
            var underlyingBinder = base.GetModelBinder(config, parameter.ParameterType);
            var binder = new JsonPathBinder(underlyingBinder, _jsonPath);
            var valueProviderFactories = GetValueProviderFactories(config);

            return new ModelBinderParameterBinding(parameter, binder, valueProviderFactories);
        }

        private class JsonPathBinder : IModelBinder
        {
            private readonly IModelBinder _underlyingBinder;
            private readonly string _jsonPath;

            public JsonPathBinder(IModelBinder underlyingBinder, string jsonPath)
            {
                _underlyingBinder = underlyingBinder;
                _jsonPath = jsonPath;
            }

            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (actionContext.Request.Method == HttpMethod.Get)
                {
                    return _underlyingBinder.BindModel(actionContext, bindingContext);
                }

                var requestContent = new HttpMessageContent(actionContext.Request);
                var strJson = requestContent.HttpRequestMessage.Content.ReadAsStringAsync().Result;

                if (strJson.IsNullOrWhiteSpace())
                {
                    return _underlyingBinder.BindModel(actionContext, bindingContext);
                }

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