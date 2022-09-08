using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <remarks>
///     <para>
///         This attribute is odd because it applies at class level where some methods may use it whilst others don't.
///     </para>
///     <para>
///         What we should probably have (if we really even need something like this at all) is an attribute for method
///         level.
///         <example>
///             <code>
/// 
/// [HasParameterFromUriOrBodyOfType("ids", typeof(Guid[]))]
/// public IActionResult GetByIds([FromJsonPath] Guid[] ids) { }
/// 
/// [HasParameterFromUriOrBodyOfType("ids", typeof(int[]))]
/// public IActionResult GetByIds([FromJsonPath] int[] ids) { }
/// </code>
///         </example>
///     </para>
///     <para>
///         That way we wouldn't need confusing things like Accept returning true when action name doesn't even match
///         attribute metadata.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class ParameterSwapControllerActionSelectorAttribute : Attribute, IActionConstraint
{
    private readonly string _actionName;
    private readonly string _parameterName;
    private readonly Type[] _supportedTypes;

    public ParameterSwapControllerActionSelectorAttribute(string actionName, string parameterName,
        params Type[] supportedTypes)
    {
        _actionName = actionName;
        _parameterName = parameterName;
        _supportedTypes = supportedTypes;
    }

    /// <inheritdoc />
    public int Order { get; set; } = 101;

    /// <inheritdoc />
    public bool Accept(ActionConstraintContext context)
    {
        if (!IsValidCandidate(context.CurrentCandidate))
        {
            // See remarks on class, required because we apply at class level
            // and some controllers have some actions with parameter swaps and others without.
            return true;
        }

        ActionSelectorCandidate? chosenCandidate = SelectAction(context);

        var found = context.CurrentCandidate.Equals(chosenCandidate);
        return found;
    }

    private ActionSelectorCandidate? SelectAction(ActionConstraintContext context)
    {
        if (TryBindFromUri(context, out ActionSelectorCandidate? candidate))
        {
            return candidate;
        }

        HttpContext httpContext = context.RouteContext.HttpContext;

        // if it's a post we can try to read from the body and bind from the json value
        if (context.RouteContext.HttpContext.Request.Method.Equals(HttpMethod.Post.Method))
        {
            JObject? postBodyJson;

            if (httpContext.Items.TryGetValue(Constants.HttpContext.Items.RequestBodyAsJObject, out var value) &&
                value is JObject cached)
            {
                postBodyJson = cached;
            }
            else
            {
                // We need to use the asynchronous method here if synchronous IO is not allowed (it may or may not be, depending
                // on configuration in UmbracoBackOfficeServiceCollectionExtensions.AddUmbraco()).
                // We can't use async/await due to the need to override IsValidForRequest, which doesn't have an async override, so going with
                // this, which seems to be the least worst option for "sync to async" (https://stackoverflow.com/a/32429753/489433).
                //
                // To expand on the above, if KestrelServerOptions/IISServerOptions is AllowSynchronousIO=false
                // And you attempt to read stream sync an InvalidOperationException is thrown with message
                // "Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead."
                var rawBody = Task.Run(() => httpContext.Request.GetRawBodyStringAsync()).GetAwaiter().GetResult();
                try
                {
                    postBodyJson = JsonConvert.DeserializeObject<JObject>(rawBody);
                    httpContext.Items[Constants.HttpContext.Items.RequestBodyAsJObject] = postBodyJson;
                }
                catch (JsonException)
                {
                    postBodyJson = null;
                }
            }

            if (postBodyJson == null)
            {
                return null;
            }

            JToken? requestParam = postBodyJson[_parameterName];

            if (requestParam != null)
            {
                Type[] paramTypes = _supportedTypes;

                foreach (Type paramType in paramTypes)
                {
                    try
                    {
                        var converted = requestParam.ToObject(paramType);
                        if (converted != null)
                        {
                            ActionSelectorCandidate? foundCandidate = MatchByType(paramType, context);
                            if (foundCandidate.HasValue)
                            {
                                return foundCandidate;
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // can't convert
                    }
                }
            }
        }

        return null;
    }

    private bool TryBindFromUri(ActionConstraintContext context, out ActionSelectorCandidate? foundCandidate)
    {
        string? requestParam = null;
        if (context.RouteContext.HttpContext.Request.Query.TryGetValue(_parameterName, out StringValues stringValues))
        {
            requestParam = stringValues.ToString();
        }

        if (requestParam is null && context.RouteContext.RouteData.Values.TryGetValue(_parameterName, out var value))
        {
            requestParam = value?.ToString();
        }

        if (requestParam == string.Empty && _supportedTypes.Length > 0)
        {
            // if it's empty then in theory we can select any of the actions since they'll all need to deal with empty or null parameters
            // so we'll try to use the first one available
            foundCandidate = MatchByType(_supportedTypes[0], context);
            if (foundCandidate.HasValue)
            {
                return true;
            }
        }

        if (requestParam != null)
        {
            foreach (Type paramType in _supportedTypes)
            {
                // check if this is IEnumerable and if so this will get it's type
                // we need to know this since the requestParam will always just be a string
                Type? enumType = paramType.GetEnumeratedType();

                Attempt<object?> converted = requestParam.TryConvertTo(enumType ?? paramType);
                if (converted.Success)
                {
                    foundCandidate = MatchByType(paramType, context);
                    if (foundCandidate.HasValue)
                    {
                        return true;
                    }
                }
            }
        }

        foundCandidate = null;
        return false;
    }

    private ActionSelectorCandidate? MatchByType(Type idType, ActionConstraintContext context)
    {
        if (context.Candidates.Count() > 1)
        {
            // choose the one that has the parameter with the T type
            ActionSelectorCandidate candidate = context.Candidates.FirstOrDefault(x =>
                x.Action.Parameters.FirstOrDefault(p => p.Name == _parameterName && p.ParameterType == idType) != null);

            return candidate;
        }

        return null;
    }

    private bool IsValidCandidate(ActionSelectorCandidate candidate)
    {
        if (!(candidate.Action is ControllerActionDescriptor controllerActionDescriptor))
        {
            return false;
        }

        if (controllerActionDescriptor.ActionName != _actionName)
        {
            return false;
        }

        return true;
    }
}
