using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UmbracoExamine.Core;
using System.Xml;



namespace umbraco.presentation.dialogs
{
    public partial class search : System.Web.UI.Page
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
            string query = keyword.Text;

            var results = ExamineManager.Instance
                .SearchProviderCollection["InternalSearch"]
                .Search(query, 100, false);

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
