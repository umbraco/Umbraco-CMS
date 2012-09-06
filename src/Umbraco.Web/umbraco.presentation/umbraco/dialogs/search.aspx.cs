using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine;
using System.Xml;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
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

            var txt = keyword.Text.ToLower();

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
            var internalSearcher = (app == "member")
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
                var operation = criteria.Field("__nodeName", txt.MultipleCharacterWildcard());

                // ensure the user can only find nodes they are allowed to see
                if (UmbracoContext.Current.UmbracoUser.StartNodeId > 0)
                {
                    operation = operation.And().Id(UmbracoContext.Current.UmbracoUser.StartNodeId);
                }

                results = internalSearcher.Search(operation.Compile());

            }

            if (results.Count() == 0)
            {
                nothingFound.Visible = true;
            }
            else
            {
                nothingFound.Visible = false;
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
