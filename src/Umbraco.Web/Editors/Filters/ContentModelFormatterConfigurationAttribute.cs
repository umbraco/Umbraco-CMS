using System;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Umbraco.Core;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Used by the content, media, members controller to bind (format) the incoming Save model
    /// </summary>
    internal class ContentModelFormatterConfigurationAttribute : Attribute, IControllerConfiguration
    {
        private readonly Type _contentModelFormatterType;

        public ContentModelFormatterConfigurationAttribute(Type contentModelFormatterType)
        {
            if (contentModelFormatterType == null) throw new ArgumentNullException("contentModelFormatterType");
            if (TypeHelper.IsTypeAssignableFrom<MediaTypeFormatter>(contentModelFormatterType) == false) throw new ArgumentException("Invalid type allowed", "contentModelFormatterType");
            _contentModelFormatterType = contentModelFormatterType;
        }

        public virtual void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            //add the multi-part formatter
            controllerSettings.Formatters.Add((MediaTypeFormatter)Activator.CreateInstance(_contentModelFormatterType));
        }
    }

}