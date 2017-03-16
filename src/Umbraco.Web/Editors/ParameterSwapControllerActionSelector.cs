using System;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// This is used to auto-select specific actions on controllers that would otherwise be ambiguous based on a single parameter type
    /// </summary>
    /// <remarks>
    /// As an example, lets say we have 2 methods: GetChildren(int id) and GetChildren(Guid id), by default Web Api won't allow this since
    /// it won't know what to select, but if this Tuple is passed in new Tuple{string, string}("GetChildren", "id")
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
                var id = HttpUtility.ParseQueryString(controllerContext.Request.RequestUri.Query).Get(found.ParamName);

                if (id != null)
                {
                    var idTypes = found.SupportedTypes;

                    foreach (var idType in idTypes)
                    {
                        var converted = id.TryConvertTo(idType);
                        if (converted)
                        {
                            var method = MatchByType(idType, controllerContext, found);
                            if (method != null)
                                return method;
                        }
                    }                   
                }
            }
            return base.SelectAction(controllerContext);
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