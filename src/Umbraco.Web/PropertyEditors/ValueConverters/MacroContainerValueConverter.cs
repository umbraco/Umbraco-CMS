﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Macros;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Macros;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Ensures macro syntax is parsed for the macro container which will work when getting the field
    /// values in any way (i.e. dynamically, using Field(), or IPublishedContent)
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MacroContainerValueConverter : PropertyValueConverterBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMacroRenderer _macroRenderer;

        public MacroContainerValueConverter(IUmbracoContextAccessor umbracoContextAccessor, IMacroRenderer macroRenderer)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _macroRenderer = macroRenderer ?? throw new ArgumentNullException(nameof(macroRenderer));
        }

        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.MacroContainer;

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (IHtmlString);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        // NOT thread-safe over a request because it modifies the
        // global UmbracoContext.Current.InPreviewMode status. So it
        // should never execute in // over the same UmbracoContext with
        // different preview modes.
        string RenderMacros(string source, bool preview)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            using (umbracoContext.ForcedPreview(preview)) // force for macro rendering
            {
                var sb = new StringBuilder();

                MacroTagParser.ParseMacros(
                    source,
                    //callback for when text block is found
                    textBlock => sb.Append(textBlock),
                    //callback for when macro syntax is found
                    (macroAlias, macroAttributes) => sb.Append(_macroRenderer.Render(
                        macroAlias,
                        umbracoContext.PublishedRequest?.PublishedContent,
                        //needs to be explicitly casted to Dictionary<string, object>
                        macroAttributes.ConvertTo(x => (string)x, x => x)).ToString()));

                return sb.ToString();
            }
         }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensure string is parsed for macros and macros are executed correctly
            sourceString = RenderMacros(sourceString, preview);

            return new HtmlString(sourceString);
        }
    }
}
