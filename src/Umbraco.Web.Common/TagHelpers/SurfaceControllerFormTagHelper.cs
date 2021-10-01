using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.Common.TagHelpers
{
    [HtmlTargetElement("form")]
    public class SurfaceControllerFormTagHelper : TagHelper
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private IDictionary<string, string> _routeValues;

        /// <summary>
        /// The name of the action method.
        /// </summary>
        [HtmlAttributeName("umb-action")]
        public string ControllerAction { get; set; }

        /// <summary>
        /// The name of the controller minus the 'Controller' suffix.
        /// </summary>
        [HtmlAttributeName("umb-controller")]
        public string ControllerName { get; set; }

        /// <summary>
        /// The name of the area.
        /// </summary>
        [HtmlAttributeName("umb-area")]
        public string Area { get; set; } = "";

        /// <summary>
        /// Additional parameters for the route.
        /// </summary>
        [HtmlAttributeName("umb-route-vals", DictionaryAttributePrefix = "umb-route-val")]
        public IDictionary<string, string> RouteValues
        {
            get
            {
                if (_routeValues == null)
                {
                    _routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                return _routeValues;
            }
            set
            {
                _routeValues = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Rendering.ViewContext"/> of the executing view.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public SurfaceControllerFormTagHelper( IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if(string.IsNullOrEmpty(ControllerName) || string.IsNullOrEmpty(ControllerAction))
                return;
            
            var encryptedString = EncryptionHelper.CreateEncryptedRouteString(_dataProtectionProvider,
                ControllerName,
                ControllerAction,
                Area,
                RouteValues);

            // Use new TagBuilder to help generate the HTML hidden input field
            // As opposed to do string formatting/concatantion 
            var surfaceControllerHiddenInput = new TagBuilder("input")
            {
                TagRenderMode = TagRenderMode.SelfClosing                
            };

            surfaceControllerHiddenInput.MergeAttribute("name", "ufprt");
            surfaceControllerHiddenInput.MergeAttribute("type", "hidden");
            surfaceControllerHiddenInput.MergeAttribute("value", encryptedString);

            // Append hidden input ufprt before closing </form>
            // The value contains a magical string on how to route the request to the surface controller
            output.PostContent.AppendHtml(surfaceControllerHiddenInput);

            // Note: We do not need to add the HTMLAntiforgeryToken
            // As the existing Microsoft Form taghelper will add it before the end of the form
        }
    }
}
