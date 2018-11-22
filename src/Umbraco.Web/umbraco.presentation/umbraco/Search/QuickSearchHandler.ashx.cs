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
using Examine.LuceneEngine.SearchCriteria;
using Umbraco.Core;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [Obsolete("This is not used and will be removed in the future")]
    public class QuickSearchHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            Authorize();

            context.Response.ContentType = "application/json";

            var txt = UmbracoContext.Current.Request["q"].ToLower();

            //the app can be Content or Media only, otherwise an exception will be thrown
            var app = UmbracoExamine.IndexTypes.Content;
            if (!string.IsNullOrEmpty(UmbracoContext.Current.Request["app"]))
            {
                app = UmbracoContext.Current.Request["app"].ToLower();
            }
            
            int limit;
            if (!int.TryParse(UmbracoContext.Current.Request["limit"], out limit))
            {
                limit = 100;
            }
            
            //if it doesn't start with "*", then search only nodeName and nodeId
            var internalSearcher = (app == Constants.Applications.Members)
                ? UmbracoContext.Current.InternalMemberSearchProvider 
                : UmbracoContext.Current.InternalSearchProvider;

            //create some search criteria, make everything combined to be 'And' and only search the current app
            var criteria = internalSearcher.CreateSearchCriteria(app, Examine.SearchCriteria.BooleanOperation.And);

            IEnumerable<SearchResult> results;
            if (txt.StartsWith("*"))
            {
                //if it starts with * then search all fields
                results = internalSearcher.Search(txt.Substring(1), true);
            }
            else
            {
				var words = txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLower().MultipleCharacterWildcard()).ToList();
                var operation = criteria.GroupedOr(new[] { "__nodeName", "__NodeId", "id" }, new[] { words[0] });
				words.RemoveAt(0);
				foreach (var word in words)
					operation = operation.And().GroupedOr(new[] { "__nodeName" }, new[] { word });

                // ensure the user can only find nodes they are allowed to see
                if (UmbracoContext.Current.UmbracoUser.StartNodeId > 0)
                {
                    operation = operation.And().Id(UmbracoContext.Current.UmbracoUser.StartNodeId);
                }

                results = internalSearcher.Search(operation.Compile());
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            context.Response.Write(js.Serialize(results.Take(limit)));
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
