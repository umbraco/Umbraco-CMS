using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.ModelBinders;

/// <summary>
///     Allows an Action to execute with an arbitrary number of QueryStrings
/// </summary>
/// <remarks>
///     Just like you can POST an arbitrary number of parameters to an Action, you can't GET an arbitrary number
///     but this will allow you to do it.
/// </remarks>
public sealed class HttpQueryStringModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        Dictionary<string, StringValues> queryStrings =
            GetQueryAsDictionary(bindingContext.ActionContext.HttpContext.Request.Query);
        var queryStringKeys = queryStrings.Select(kvp => kvp.Key).ToArray();
        if (queryStringKeys.InvariantContains("culture") == false)
        {
            queryStrings.Add(
                "culture",
                new StringValues(bindingContext.ActionContext.HttpContext.Request.ClientCulture()));
        }

        var formData = new FormCollection(queryStrings);
        bindingContext.Result = ModelBindingResult.Success(formData);
        return Task.CompletedTask;
    }

    private Dictionary<string, StringValues> GetQueryAsDictionary(IQueryCollection? query)
    {
        var result = new Dictionary<string, StringValues>();
        if (query == null)
        {
            return result;
        }

        foreach (KeyValuePair<string, StringValues> item in query)
        {
            result.Add(item.Key, item.Value);
        }

        return result;
    }
}
