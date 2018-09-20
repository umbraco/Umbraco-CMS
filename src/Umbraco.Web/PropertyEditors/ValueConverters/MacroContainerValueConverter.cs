using System;
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
        private readonly ServiceContext _services;
        private readonly CacheHelper _appCache;

        public MacroContainerValueConverter(IUmbracoContextAccessor umbracoContextAccessor, ServiceContext services, CacheHelper appCache)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _appCache = appCache ?? throw new ArgumentNullException(nameof(appCache));
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias == Constants.PropertyEditors.Aliases.MacroContainer;

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (IHtmlString);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
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

                var umbracoHelper = new UmbracoHelper(umbracoContext, _services, _appCache);
                MacroTagParser.ParseMacros(
                    source,
                    //callback for when text block is found
                    textBlock => sb.Append(textBlock),
                    //callback for when macro syntax is found
                    (macroAlias, macroAttributes) => sb.Append(umbracoHelper.RenderMacro(
                        macroAlias,
                        //needs to be explicitly casted to Dictionary<string, object>
                        macroAttributes.ConvertTo(x => (string)x, x => x)).ToString()));

                return sb.ToString();
            }
         }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // ensure string is parsed for macros and macros are executed correctly
            sourceString = RenderMacros(sourceString, preview);

            return new HtmlString(sourceString);
        }
    }
}
