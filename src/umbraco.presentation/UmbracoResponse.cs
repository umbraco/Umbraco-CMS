using System;
using System.Web;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation
{
    public class UmbracoResponse : HttpResponseWrapper
    {
        public UmbracoResponse(HttpResponse response) : base(response)
        {
        }
    }
}
