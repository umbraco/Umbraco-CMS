using System;
using Umbraco.Web.Mvc;
using Umbraco.Core;
using System.Web.Helpers;
using System.Web;
using Newtonsoft.Json;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// A custom <see cref="IAntiForgeryAdditionalDataProvider"/> to create a unique antiforgery token/validator per form created with BeginUmbracoForm
    /// </summary>
    public class UmbracoAntiForgeryAdditionalDataProvider : IAntiForgeryAdditionalDataProvider
    {
        private readonly IAntiForgeryAdditionalDataProvider _defaultProvider;

        /// <summary>
        /// Constructor, allows wrapping a default provider
        /// </summary>
        /// <param name="defaultProvider"></param>
        public UmbracoAntiForgeryAdditionalDataProvider(IAntiForgeryAdditionalDataProvider defaultProvider)
        {
            _defaultProvider = defaultProvider;
        }

        public string GetAdditionalData(HttpContextBase context)
        {
            return JsonConvert.SerializeObject(new AdditionalData
            {
                Stamp = DateTime.UtcNow.Ticks,
                //this value will be here if this is a BeginUmbracoForms form
                Ufprt = context.Items["ufprt"]?.ToString(),
                //if there was a wrapped provider, add it's value to the json, else just a static value
                WrappedValue = _defaultProvider?.GetAdditionalData(context) ?? "default"
            });
        }

        public bool ValidateAdditionalData(HttpContextBase context, string additionalData)
        {
            if (!additionalData.DetectIsJson())
                return false; //must be json

            AdditionalData json;
            try
            {
                json = JsonConvert.DeserializeObject<AdditionalData>(additionalData);
            }
            catch
            {
                return false; //couldn't parse
            }

            if (json.Stamp == default) return false;

            //if there was a wrapped provider, validate it, else validate the static value
            var validateWrapped = _defaultProvider?.ValidateAdditionalData(context, json.WrappedValue) ?? json.WrappedValue == "default";
            if (!validateWrapped)
                return false;

            var ufprtRequest = context.Request["ufprt"]?.ToString();

            //if the custom BeginUmbracoForms route value is not there, then it's nothing more to validate
            if (ufprtRequest.IsNullOrWhiteSpace() && json.Ufprt.IsNullOrWhiteSpace())
                return true;

            //if one or the other is null then something is wrong
            if (!ufprtRequest.IsNullOrWhiteSpace() && json.Ufprt.IsNullOrWhiteSpace()) return false;
            if (ufprtRequest.IsNullOrWhiteSpace() && !json.Ufprt.IsNullOrWhiteSpace()) return false;

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(json.Ufprt, out var additionalDataParts))
                return false;

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(ufprtRequest, out var requestParts))
                return false;

            //ensure they all match
            return additionalDataParts.Count == requestParts.Count
                && additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Controller] == requestParts[RenderRouteHandler.ReservedAdditionalKeys.Controller]
                && additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Action] == requestParts[RenderRouteHandler.ReservedAdditionalKeys.Action]
                && additionalDataParts[RenderRouteHandler.ReservedAdditionalKeys.Area] == requestParts[RenderRouteHandler.ReservedAdditionalKeys.Area];
        }

        internal class AdditionalData
        {
            public string Ufprt { get; set; }
            public long Stamp { get; set; }
            public string WrappedValue { get; set; }
        }

    }
}
