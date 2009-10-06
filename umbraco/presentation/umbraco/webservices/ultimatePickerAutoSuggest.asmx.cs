using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Web.Script.Services;
using umbraco;
using umbraco.BasePages;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;



namespace umbraco.presentation.umbraco.webservices
{
    /// <summary>
    /// Summary description for ultimatePickerAutoSuggest
    /// </summary>

    [WebService(Namespace = "http://umbraco.org/webservices")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class ultimatePickerAutoSuggest : System.Web.Services.WebService
    {
        private string temp;
        private int nodeCount;
        private int Counter;
        private string[] output;
        private string prefix;

        [WebMethod]
        public string[] getNodes(string prefixText, int count, string contextKey)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                prefix = prefixText;
                string[] config = contextKey.Split("|".ToCharArray());
                int parentNodeId = Convert.ToInt32(config[0]);
                bool showGrandChildren = Convert.ToBoolean(config[1]);

                string documentAliasFilter = config[2];
                string[] documentAliasFilters = documentAliasFilter.Split(",".ToCharArray());
                
                
                CMSNode parent = new CMSNode(parentNodeId);
                if (!showGrandChildren)
                {
                    nodeCount = 0;

                    //store children array here because iterating over an Array property object is very inneficient.
                    var children = parent.Children;
                    foreach (CMSNode child in children)
                    {
                     
                       
                        nodeChildrenCount(child, false,documentAliasFilters);
                        
                    }
                  
                    output = new string[nodeCount];

                    int i = 0;
                    Counter = 0;
                    int level = 1;

                    //why is there a 2nd iteration of the same thing here?
                    foreach (CMSNode child in children)
                    {
                        
                        addNode(child, level, showGrandChildren,documentAliasFilters);
                    }

                    return output;
                }
                else
                {
                    nodeCount = 0;

                    //store children array here because iterating over an Array property object is very inneficient.
                    var children = parent.Children;
                    foreach (CMSNode child in children)
                    {
                        nodeChildrenCount(child, true,documentAliasFilters);
                    }

                    output = new string[nodeCount];
                    Counter = 0;
                    int level = 1;

                    foreach (CMSNode child in children)
                    {
                        addNode(child, level, showGrandChildren,documentAliasFilters);
                    }

                    return output;

                }

            }
            return new string[1];


        }

        private bool validNode(string nodeText)
        {


            if (nodeText.Length >= prefix.Length)
            {


                if (nodeText.Substring(0, prefix.Length).ToLower() == prefix.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        private void nodeChildrenCount(CMSNode node, bool countChildren, string[] documentAliasFilters)
        {
            if (documentAliasFilters.Length > 0)
            {

                foreach (string filter in documentAliasFilters)
                {
                    string trimmedFilter = filter.TrimStart(" ".ToCharArray());
                    trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                    if (new Document(node.Id).ContentType.Alias == trimmedFilter || trimmedFilter == string.Empty)
                    {
                        if (validNode(node.Text))
                        {
                            nodeCount += 1;
                        }

                    }
                }
            }
            else
            {
                if (validNode(node.Text))
                {
                    nodeCount += 1;
                }
            }
            
            if (countChildren)
            {
                //store children array here because iterating over an Array property object is very inneficient.
                var children = node.Children;
                foreach (CMSNode child in children)
                {
                    nodeChildrenCount(child, countChildren,documentAliasFilters);
                }
            }

        }

        private void addNode(CMSNode node, int level, bool showGrandChildren, string[] documentAliasFilters)
        {

            string preText = string.Empty;

            for (int i = 1; i < level; i++)
            {
                preText += "- ";
            }

            if (documentAliasFilters.Length > 0)
            {
                foreach (string filter in documentAliasFilters)
                {
                    string trimmedFilter = filter.TrimStart(" ".ToCharArray());
                    trimmedFilter = trimmedFilter.TrimEnd(" ".ToCharArray());

                    if (new Document(node.Id).ContentType.Alias == trimmedFilter || trimmedFilter == string.Empty)
                    {
                        if (validNode(node.Text))
                        {
                            output[Counter] = preText + node.Text + "|" + node.Id;
                            Counter++;
                        }
                    }

                }
            }
            else
            {
                if (validNode(node.Text))
                {
                    output[Counter] = preText + node.Text + "|" + node.Id;
                    Counter++;
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
                        addNode(child, level + 1, showGrandChildren,documentAliasFilters);
                    }
                }
            }
        }
    }
}
