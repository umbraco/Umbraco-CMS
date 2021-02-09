using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Core;
using Umbraco.Extensions;

namespace Umbraco.Web.BackOffice.Controllers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal class ParameterSwapControllerActionSelectorAttribute : Attribute, IActionConstraint
    {
        private readonly string _actionName;
        private readonly string _parameterName;
        private readonly Type[] _supportedTypes;
        private string _requestBody;

        public ParameterSwapControllerActionSelectorAttribute(string actionName, string parameterName, params Type[] supportedTypes)
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
                return true;
            }

            var chosenCandidate = SelectAction(context);

            var found = context.CurrentCandidate.Equals(chosenCandidate);
            return found;
        }

        private ActionSelectorCandidate? SelectAction(ActionConstraintContext context)
        {
            if (TryBindFromUri(context, out var candidate))
            {
                return candidate;
            }

            // if it's a post we can try to read from the body and bind from the json value
            if (context.RouteContext.HttpContext.Request.Method == HttpMethod.Post.ToString())
            {
                // We need to use the asynchronous method here if synchronous IO is not allowed (it may or may not be, depending
                // on configuration in UmbracoBackOfficeServiceCollectionExtensions.AddUmbraco()).
                // We can't use async/await due to the need to override IsValidForRequest, which doesn't have an async override, so going with
                // this, which seems to be the least worst option for "sync to async" (https://stackoverflow.com/a/32429753/489433).
                var strJson = _requestBody ??= Task.Run(() => context.RouteContext.HttpContext.Request.GetRawBodyStringAsync()).GetAwaiter().GetResult();

                var json = JsonConvert.DeserializeObject<JObject>(strJson);

                if (json == null)
                {
                    return null;
                }

                var requestParam = json[_parameterName];

                if (requestParam != null)
                {
                    var paramTypes = _supportedTypes;

                    foreach (var paramType in paramTypes)
                    {
                        try
                        {
                            var converted = requestParam.ToObject(paramType);
                            if (converted != null)
                            {
                                var foundCandidate = MatchByType(paramType, context);
                                if (foundCandidate.HasValue)
                                {
                                    return foundCandidate;
                                }
                            }
                        }
                        catch (JsonReaderException)
                        {
                            // can't convert
                        }
                        catch (JsonSerializationException)
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

            string requestParam = null;
            if (context.RouteContext.HttpContext.Request.Query.TryGetValue(_parameterName, out var stringValues))
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
                foreach (var paramType in _supportedTypes)
                {
                    // check if this is IEnumerable and if so this will get it's type
                    // we need to know this since the requestParam will always just be a string
                    var enumType = paramType.GetEnumeratedType();

                    var converted = requestParam.TryConvertTo(enumType ?? paramType);
                    if (converted)
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
                var candidate = context.Candidates.FirstOrDefault(x => x.Action.Parameters.FirstOrDefault(p => p.Name == _parameterName && p.ParameterType == idType) != null);

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
}
