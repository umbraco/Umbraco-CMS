using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelBinders;

/// <summary>
///     Used to bind a value from an inner json property
/// </summary>
/// <remarks>
///     An example would be if you had json like:
///     { ids: [1,2,3,4] }
///     And you had an action like: GetByIds(int[] ids, UmbracoEntityTypes type)
///     The ids array will not bind because the object being sent up is an object and not an array so the
///     normal json formatter will not figure this out.
///     This would also let you bind sub levels of the JSON being sent up too if you wanted with any jsonpath
/// </remarks>
public class FromJsonPathAttribute : ModelBinderAttribute
{
    public FromJsonPathAttribute() : base(typeof(JsonPathBinder))
    {
    }

    internal class JsonPathBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.HttpContext.Request.Method.Equals(HttpMethod.Get.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (TryModelBindFromHttpContextItems(bindingContext))
            {
                return;
            }

            var strJson = await bindingContext.HttpContext.Request.GetRawBodyStringAsync();


            if (string.IsNullOrWhiteSpace(strJson))
            {
                return;
            }

            JObject? json = JsonConvert.DeserializeObject<JObject>(strJson);

            //if no explicit json path then use the model name
            JToken? match = json?.SelectToken(bindingContext.FieldName ?? bindingContext.ModelName);

            if (match == null)
            {
                return;
            }

            var model = match.ToObject(bindingContext.ModelType);

            bindingContext.Result = ModelBindingResult.Success(model);
        }

        public static bool TryModelBindFromHttpContextItems(ModelBindingContext bindingContext)
        {
            const string key = Constants.HttpContext.Items.RequestBodyAsJObject;

            if (!bindingContext.HttpContext.Items.TryGetValue(key, out var cached))
            {
                return false;
            }

            if (cached is not JObject json)
            {
                return false;
            }

            JToken? match = json.SelectToken(bindingContext.FieldName);

            // ReSharper disable once InvertIf
            if (match != null)
            {
                bindingContext.Result = ModelBindingResult.Success(match.ToObject(bindingContext.ModelType));
            }

            return true;
        }
    }
}
