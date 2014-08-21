using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using UmbracoExamine;
using System.Xml;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using System.Linq;
using Umbraco.Core;


namespace umbraco.presentation.dialogs
{
    public partial class search : BasePages.UmbracoEnsuredPage
    {

        protected override void OnInit(EventArgs e)
        {
            CurrentApp = IndexTypes.Content;
            if (!string.IsNullOrEmpty(Request["app"]))
            {
                CurrentApp = Request["app"].ToLower();
            }

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Form.DefaultButton = searchButton.UniqueID;

            if (!IsPostBack && Request["search"] != "")
            {
                keyword.Text = Request["search"];
                DoSearch();

            }
        }

        protected void search_Click(object sender, EventArgs e)
        {
            DoSearch();            
        }

        private void DoSearch()
        {
            var txt = keyword.Text.ToLower();

            int limit;
            if (!int.TryParse(Request["limit"], out limit))
            {
                limit = 100;
            }

            //if it doesn't start with "*", then search only nodeName and nodeId
            var internalSearcher = (CurrentApp == Constants.Applications.Members)
                ? UmbracoContext.Current.InternalMemberSearchProvider
                : UmbracoContext.Current.InternalSearchProvider;

            //create some search criteria, make everything combined to be 'And' and only search the current app
            var criteria = internalSearcher.CreateSearchCriteria(CurrentApp, Examine.SearchCriteria.BooleanOperation.And);

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
                    //TODO: This is not correct! This will not filter out seearches 'from' this node, this
                    // query is meant to search 'for' a specific node.
                    operation = operation.And().Id(UmbracoContext.Current.UmbracoUser.StartNodeId);
                }

                results = internalSearcher.Search(operation.Compile());
            }

            nothingFound.Visible = !results.Any();

            searchResult.XPathNavigator = ResultsAsXml(results).CreateNavigator();
        }

        private XmlDocument ResultsAsXml(IEnumerable<SearchResult> results)
        {
            var result = new XmlDocument();
            result.LoadXml("<results/>");
            
            foreach (var r in results)
            {
                var x = XmlHelper.AddTextNode(result, "result", "");
                x.Attributes.Append(XmlHelper.AddAttribute(result, "id", r.Id.ToString(CultureInfo.InvariantCulture)));
                x.Attributes.Append(XmlHelper.AddAttribute(result, "title", r.Fields["nodeName"]));
                x.Attributes.Append(XmlHelper.AddAttribute(result, "author", r.Fields["writerName"]));
                x.Attributes.Append(XmlHelper.AddAttribute(result, "changeDate", r.Fields["updateDate"]));
                x.Attributes.Append(xmlHelper.addAttribute(result, "type", r.Fields["nodeTypeAlias"]));
                result.DocumentElement.AppendChild(x);
            }

            return result;
        }
    }
}
