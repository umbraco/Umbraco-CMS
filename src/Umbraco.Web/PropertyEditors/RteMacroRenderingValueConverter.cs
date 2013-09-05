using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.PropertyEditors
{
	/// <summary>
	/// A value converter for TinyMCE that will ensure any macro content is rendered properly even when 
	/// used dynamically.
	/// </summary>
    // because that version of RTE converter parses {locallink} and executes macros, when going from
    // data to source, its source value has to be cached at the request level, because we have no idea
    // what the macros may depend on actually. An so, object and xpath need to follow... request, too.
    // note: the TinyMceValueConverter is NOT inherited, so the PropertyValueCache attribute here is not
    // actually required (since Request is default) but leave it here to be absolutely explicit.
    [PropertyValueType(typeof(IHtmlString))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Request)]
    internal class RteMacroRenderingValueConverter : TinyMceValueConverter
	{
        string RenderRteMacros(string source)
        {
            // fixme - not taking 'preview' into account here
            // but we should, when running the macros... how?!

            var sb = new StringBuilder();
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            MacroTagParser.ParseMacros(
                source,
                //callback for when text block is found
                textBlock => sb.Append(textBlock),
                //callback for when macro syntax is found
                (macroAlias, macroAttributes) => sb.Append(umbracoHelper.RenderMacro(
                    macroAlias,
                    //needs to be explicitly casted to Dictionary<string, object>
                    macroAttributes.ConvertTo(x => (string)x, x => (object)x)).ToString()));
            return sb.ToString();
        }

	    public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
	    {
	        if (source == null) return null;
	        var sourceString = source.ToString();

            sourceString = TextValueConverterHelper.ParseStringValueSource(sourceString); // fixme - must handle preview
            sourceString = RenderRteMacros(sourceString); // fixme - must handle preview

	        return sourceString;
	    }
    }
}