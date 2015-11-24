using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A custom ActionMethodSelector which will ensure that the RenderMvcController.Index(RenderModel model) action will be executed
    /// if the 
    /// </summary>
    internal class RenderIndexActionSelectorAttribute : ActionMethodSelectorAttribute
    {
        private static readonly ConcurrentDictionary<Type, ReflectedControllerDescriptor> ControllerDescCache = new ConcurrentDictionary<Type, ReflectedControllerDescriptor>();

        /// <summary>
        /// Determines whether the action method selection is valid for the specified controller context.
        /// </summary>
        /// <returns>
        /// true if the action method selection is valid for the specified controller context; otherwise, false.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="methodInfo">Information about the action method.</param>
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            var currType = methodInfo.ReflectedType;
            var baseType = methodInfo.DeclaringType;

            //It's the same type, so this must be the Index action to use
            if (currType == baseType) return true;

            if (currType == null) return false;

            var controllerDesc = ControllerDescCache.GetOrAdd(currType, type => new ReflectedControllerDescriptor(currType));
            var actions = controllerDesc.GetCanonicalActions();

            //If there are more than one Index action for this controller, then 
            // this base class one should not be matched
            return actions.Count(x => x.ActionName == "Index") <= 1;
        }
    }
}