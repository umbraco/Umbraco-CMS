using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation
{
    public class UmbracoRequest : HttpRequestWrapper
    {
        public UmbracoRequest(HttpRequest request) : base(request)
        {

        }

        /// <summary>
        /// Gets a value indicating whether the request has debugging enabled
        /// </summary>
        /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
        public bool IsDebug
        {
            get
            {
                return GlobalSettings.DebugMode && (!string.IsNullOrEmpty(this["umbdebugshowtrace"]) || !string.IsNullOrEmpty(this["umbdebug"]));
            }
        }
    }
}
