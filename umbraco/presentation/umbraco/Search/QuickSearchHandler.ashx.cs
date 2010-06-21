using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Collections.Generic;
using UmbracoExamine;
using System.Web.Script.Serialization;
using Examine;

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
            Authorize();

            context.Response.ContentType = "application/json";

            var txt = UmbracoContext.Current.Request["q"].ToLower();

            //the app can be Content or Media only, otherwise an exception will be thrown
            var app = "Content";
            if (!string.IsNullOrEmpty(UmbracoContext.Current.Request["app"]))
            {
                app = UmbracoContext.Current.Request["app"];
            }
            
            int limit;
            if (!int.TryParse(UmbracoContext.Current.Request["limit"], out limit))
            {
                limit = 100;
            }


            //if it doesn't start with "*", then search only nodeName and nodeId
            var internalSearcher = (app == "Member")
                ? UmbracoContext.Current.InternalMemberSearchProvider 
                : UmbracoContext.Current.InternalSearchProvider;
            var criteria = internalSearcher.CreateSearchCriteria(app);
            IEnumerable<SearchResult> results;
            if (txt.StartsWith("*"))
            {
                results = internalSearcher.Search("*", true);
            }
            else
            {
                var operation = criteria.NodeName(txt);
                if (UmbracoContext.Current.UmbracoUser.StartNodeId > 0)
                {
                    operation.Or().Id(UmbracoContext.Current.UmbracoUser.StartNodeId);
                }

                results = internalSearcher.Search(operation.Compile());
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            context.Response.Write(js.Serialize(results.Take(100)));
        }

        public static void Authorize()
        {
            if (!BasePages.BasePage.ValidateUserContextID(BasePages.BasePage.umbracoUserContextID))
                throw new Exception("Client authorization failed. User is not logged in");

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
