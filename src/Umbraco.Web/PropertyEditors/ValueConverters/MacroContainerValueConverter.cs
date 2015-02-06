using System;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Ensures macro syntax is parsed for the macro container which will work when getting the field
    /// values in any way (i.e. dynamically, using Field(), or IPublishedContent)
    /// </summary>
    [PropertyValueType(typeof (IHtmlString))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Request)]
    [DefaultPropertyValueConverter]
    public class MacroContainerValueConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias == Constants.PropertyEditors.MacroContainerAlias;
        }

        // NOT thread-safe over a request because it modifies the
        // global UmbracoContext.Current.InPreviewMode status. So it
        // should never execute in // over the same UmbracoContext with
        // different preview modes.
        static string RenderMacros(string source, bool preview)
        {
            // save and set for macro rendering
            var inPreviewMode = UmbracoContext.Current.InPreviewMode;
            UmbracoContext.Current.InPreviewMode = preview;

            var sb = new StringBuilder();

            try
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                MacroTagParser.ParseMacros(
                    source,
                    //callback for when text block is found
                    textBlock => sb.Append(textBlock),
                    //callback for when macro syntax is found
                    (macroAlias, macroAttributes) => sb.Append(umbracoHelper.RenderMacro(
                        macroAlias,
                        //needs to be explicitly casted to Dictionary<string, object>
                        macroAttributes.ConvertTo(x => (string)x, x => x)).ToString()));
            }
            finally
            {
                // restore
                UmbracoContext.Current.InPreviewMode = inPreviewMode;
            }

            return sb.ToString();
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensure string is parsed for macros and macros are executed correctly
            sourceString = RenderMacros(sourceString, preview);

            return sourceString;
        }
    }
}