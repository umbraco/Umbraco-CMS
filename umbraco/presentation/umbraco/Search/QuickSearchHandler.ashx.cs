using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Collections.Generic;
using UmbracoExamine.Core;
using System.Web.Script.Serialization;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class QuickSearchHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            
            IEnumerable<SearchResult> results = ExamineManager.Instance
                .SearchProviderCollection["InternalSearch"]
                .Search(UmbracoContext.Current.Request["q"], 20, false);

            JavaScriptSerializer js = new JavaScriptSerializer();
            context.Response.Write(js.Serialize(results));
        }



        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
