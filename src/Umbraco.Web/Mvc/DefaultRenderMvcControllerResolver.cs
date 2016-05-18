using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Plugins;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A resolver used to resolve the default RenderMvcController that is used to render any front-end
    /// Umbraco page when using MVC when there are no routes hijacked.
    /// </summary>
    public class DefaultRenderMvcControllerResolver : SingleObjectResolverBase<DefaultRenderMvcControllerResolver, Type>
    {
        /// <summary>
        /// Constructor accepting the default RenderMvcController
        /// </summary>
        /// <param name="value"></param>
        public DefaultRenderMvcControllerResolver(Type value)
            : base(value)
        {
            ValidateType(value);
        }

        /// <summary>
        /// Returns the Default RenderMvcController type
        /// </summary>
        /// <returns></returns>
        public Type GetDefaultControllerType()
        {
            return Value;
        }

        /// <summary>
        /// Sets the default RenderMvcController type
        /// </summary>
        /// <param name="controllerType"></param>
        public void SetDefaultControllerType(Type controllerType)
        {
            ValidateType(controllerType);
            Value = controllerType;
        }

        /// <summary>
        /// Ensures that the type passed in is of type RenderMvcController
        /// </summary>
        /// <param name="type"></param>
        private void ValidateType(Type type)
        {
            if (TypeHelper.IsTypeAssignableFrom<IRenderController>(type) == false)
            {
                throw new InvalidOperationException("The Type specified (" + type + ") is not of type " + typeof (IRenderController));
            }
        }

    }
}
