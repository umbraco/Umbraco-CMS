using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// This is used to auto-select specific actions on controllers that would otherwise be ambiguous based on a single parameter type
    /// </summary>
    /// <remarks>
    /// As an example, lets say we have 2 methods: GetChildren(int id) and GetChildren(Guid id), by default Web Api won't allow this since
    /// it won't know what to select, but if this Tuple is passed in new Tuple{string, string}("GetChildren", "id")
    ///
    /// This supports POST values too however only for JSON values
    /// </remarks>
    internal class ParameterSwapControllerActionSelector : ApiControllerActionSelector
    {
        private readonly ParameterSwapInfo[] _actions;

        /// <summary>
        /// Constructor accepting a list of action name + parameter name
        /// </summary>
        /// <param name="actions"></param>
        public ParameterSwapControllerActionSelector(params ParameterSwapInfo[] actions)
        {
            _actions = actions;
        }

        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            var found = _actions.FirstOrDefault(x => controllerContext.Request.RequestUri.GetLeftPart(UriPartial.Path).InvariantEndsWith(x.ActionName));

            if (found != null)
            {
                HttpActionDescriptor method;
                if (TryBindFromUri(controllerContext, found, out method))
                {
                    return method;
                }

                //if it's a post we can try to read from the body and bind from the json value
                if (controllerContext.Request.Method == HttpMethod.Post)
                {
                    var requestContent = new HttpMessageContent(controllerContext.Request);
                    var strJson = requestContent.HttpRequestMessage.Content.ReadAsStringAsync().Result;
                    var json = JsonConvert.DeserializeObject<JObject>(strJson);

                    if (json == null)
                    {
                        return base.SelectAction(controllerContext);
                    }

                    var requestParam = json[found.ParamName];

                    if (requestParam != null)
                    {
                        var paramTypes = found.SupportedTypes;

                        foreach (var paramType in paramTypes)
                        {
                            try
                            {
                                var converted = requestParam.ToObject(paramType);
                                if (converted != null)
                                {
                                    method = MatchByType(paramType, controllerContext, found);
                                    if (method != null)
                                        return method;
                                }
                            }
                            catch (JsonReaderException)
                            {
                                //can't convert
                            }
                            catch (JsonSerializationException)
                            {
                                //can't convert
                            }
                        }
                    }
                }
            }
            return base.SelectAction(controllerContext);
        }

        private bool TryBindFromUri(HttpControllerContext controllerContext, ParameterSwapInfo found, out HttpActionDescriptor method)
        {
            var requestParam = HttpUtility.ParseQueryString(controllerContext.Request.RequestUri.Query).Get(found.ParamName);

            requestParam = (requestParam == null) ? null : requestParam.Trim();
            var paramTypes = found.SupportedTypes;

            if (requestParam == string.Empty && paramTypes.Length > 0)
            {
                //if it's empty then in theory we can select any of the actions since they'll all need to deal with empty or null parameters
                //so we'll try to use the first one available
                method = MatchByType(paramTypes[0], controllerContext, found);
                if (method != null)
                    return true;
            }

            if (requestParam != null)
            {
                foreach (var paramType in paramTypes)
                {
                    //check if this is IEnumerable and if so this will get it's type
                    //we need to know this since the requestParam will always just be a string
                    var enumType = paramType.GetEnumeratedType();

                    var converted = requestParam.TryConvertTo(enumType ?? paramType);
                    if (converted)
                    {
                        method = MatchByType(paramType, controllerContext, found);
                        if (method != null)
                            return true;
                    }
                }
            }

            method = null;
            return false;
        }

        private static ReflectedHttpActionDescriptor MatchByType(Type idType, HttpControllerContext controllerContext, ParameterSwapInfo found)
        {
            var controllerType = controllerContext.Controller.GetType();
            var methods = controllerType.GetMethods().Where(info => info.Name == found.ActionName).ToArray();
            if (methods.Length > 1)
            {
                //choose the one that has the parameter with the T type
                var method = methods.FirstOrDefault(x => x.GetParameters().FirstOrDefault(p => p.Name == found.ParamName && p.ParameterType == idType) != null);

                return new ReflectedHttpActionDescriptor(controllerContext.ControllerDescriptor, method);
            }
            return null;
        }

        internal class ParameterSwapInfo
        {
            public string ActionName { get; private set; }
            public string ParamName { get; private set; }
            public Type[] SupportedTypes { get; private set; }

            public ParameterSwapInfo(string actionName, string paramName, params Type[] supportedTypes)
            {
                ActionName = actionName;
                ParamName = paramName;
                SupportedTypes = supportedTypes;
            }
        }

    }
}
