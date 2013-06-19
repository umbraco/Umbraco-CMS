using System;
using System.Linq;
using System.Web;
using System.Web.Services;
using Umbraco.Web.WebServices;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.umbraco.webservices
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class UltimatePickerAutoCompleteHandler : UmbracoAuthorizedHttpHandler
    {

        private int _nodeCount;
        private int _counter;
        private string[] _output;
        private string _prefix;

        public override void ProcessRequest(HttpContext context)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID) == false)
                throw new Exception("Client authorization failed. User is not logged in");

            //user must be allowed to see content or media
            if (AuthorizeRequest(DefaultApps.content.ToString()) == false && AuthorizeRequest(DefaultApps.media.ToString()) == false)
                return;

            context.Response.ContentType = "text/plain";

            _prefix = context.Request.QueryString["q"];

            var parentNodeId = Convert.ToInt32(context.Request.QueryString["id"]);
            var showGrandChildren = Convert.ToBoolean(context.Request.QueryString["showchildren"]);

            var documentAliasFilter = context.Request.QueryString["filter"];
            var documentAliasFilters = documentAliasFilter.Split(",".ToCharArray());

            var parent = new CMSNode(parentNodeId);

            _nodeCount = 0;

            //store children array here because iterating over an Array property object is very inneficient.
            var children = parent.Children;
            foreach (CMSNode child in children)
            {
                NodeChildrenCount(child, showGrandChildren, documentAliasFilters);
            }

            _output = new string[_nodeCount];
            _counter = 0;
            int level = 1;

            foreach (CMSNode child in children)
            {
                AddNode(child, level, showGrandChildren, documentAliasFilters);
            }

            foreach (var item in _output)
            {
                context.Response.Write(item + Environment.NewLine);
            }
        }

        private bool ValidNode(string nodeText)
        {
            return nodeText.Length >= _prefix.Length && nodeText.Substring(0, _prefix.Length).ToLower() == _prefix.ToLower();
        }

        private void NodeChildrenCount(CMSNode node, bool countChildren, string[] documentAliasFilters)
        {
            if (documentAliasFilters.Length > 0)
            {
                foreach (var filter in documentAliasFilters)
                {
                    var trimmedFilter = filter.TrimStart(" ".ToCharArray());
                    trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                    if ((new Document(node.Id).ContentType.Alias == trimmedFilter || trimmedFilter == string.Empty) && ValidNode(node.Text))
                    {
                        _nodeCount += 1;
                    }
                }
            }
            else
            {
                if (ValidNode(node.Text))
                {
                    _nodeCount += 1;
                }
            }

            if (countChildren)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var children = node.Children;
                foreach (CMSNode child in children)
                {
                    NodeChildrenCount(child, countChildren, documentAliasFilters);
                }
            }

        }

        private void AddNode(CMSNode node, int level, bool showGrandChildren, string[] documentAliasFilters)
        {
            var preText = string.Empty;

            for (var i = 1; i < level; i++)
            {
                preText += "- ";
            }

            if (documentAliasFilters.Length > 0)
            {
                foreach (var filter in documentAliasFilters)
                {
                    var trimmedFilter = filter.TrimStart(" ".ToCharArray());
                    trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                    if ((new Document(node.Id).ContentType.Alias == trimmedFilter || trimmedFilter == string.Empty) && ValidNode(node.Text))
                    {
                        _output[_counter] = preText + node.Text + " [" + node.Id + "]";
                        _counter++;
                    }

                }
            }
            else
            {
                if (ValidNode(node.Text))
                {
                    _output[_counter] = preText + node.Text + " [" + node.Id + "]";
                    _counter++;
                }
            }

            if (showGrandChildren)
            {
                if (node.HasChildren)
                {
                    //store children array here because iterating over an Array property object is very inneficient.
                    var children = node.Children;
                    foreach (CMSNode child in children)
                    {
                        AddNode(child, level + 1, showGrandChildren, documentAliasFilters);
                    }
                }
            }
        }

        public override bool IsReusable
        {
            get { return false; }
        }
    }
}
