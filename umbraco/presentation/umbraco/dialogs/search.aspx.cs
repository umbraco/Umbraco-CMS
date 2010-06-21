using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine;
using System.Xml;
using Examine;
using System.Linq;


namespace umbraco.presentation.dialogs
{
    public partial class search : BasePages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.Form.DefaultButton = this.searchButton.UniqueID;

            if (!IsPostBack && UmbracoContext.Current.Request["search"] != "")
            {
                keyword.Text = UmbracoContext.Current.Request["search"];
                doSearch();

            }
        }

        protected void search_Click(object sender, EventArgs e)
        {
            doSearch();


        }

        private void doSearch()
        {

            int limit;
            if (!int.TryParse(UmbracoContext.Current.Request["limit"], out limit))
            {
                limit = 100;
            }

            string query = keyword.Text.ToLower();

            //the app can be Content or Media only, otherwise an exception will be thrown
            var app = "Content";
            if (!string.IsNullOrEmpty(UmbracoContext.Current.Request["app"]))
            {
                app = UmbracoContext.Current.Request["app"];
            }
            
            //if it doesn't start with "*", then search only nodeName and nodeId
            var internalSearcher = (app == "Member")
                ? UmbracoContext.Current.InternalMemberSearchProvider
                : UmbracoContext.Current.InternalSearchProvider;
            var criteria = internalSearcher.CreateSearchCriteria(app);
            IEnumerable<SearchResult> results;
            if (query.StartsWith("*"))
            {
                results = internalSearcher.Search("*", true);
            }
            else
            {
                var operation = criteria.NodeName(query);
                if (UmbracoContext.Current.UmbracoUser.StartNodeId > 0)
                {
                    operation.Or().Id(UmbracoContext.Current.UmbracoUser.StartNodeId);
                }

                results = internalSearcher.Search(operation.Compile()).Take(limit);
            }

            searchResult.XPathNavigator = ResultsAsXml(results).CreateNavigator();
        }

        private XmlDocument ResultsAsXml(IEnumerable<SearchResult> results)
        {
            XmlDocument result = new XmlDocument();
            result.LoadXml("<results/>");
            
            foreach (var r in results)
            {
                XmlNode x = xmlHelper.addTextNode(result, "result", "");
                x.Attributes.Append(xmlHelper.addAttribute(result, "id", r.Id.ToString()));
                x.Attributes.Append(xmlHelper.addAttribute(result, "title", r.Fields["nodeName"]));
                x.Attributes.Append(xmlHelper.addAttribute(result, "author", r.Fields["writerName"]));
                x.Attributes.Append(xmlHelper.addAttribute(result, "changeDate", r.Fields["updateDate"]));
                result.DocumentElement.AppendChild(x);
            }

            return result;
        }
    }
}
