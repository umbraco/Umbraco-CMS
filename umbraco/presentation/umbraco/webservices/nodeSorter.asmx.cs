using System;
using System.Collections;
using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using umbraco.BasePages;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for nodeSorter
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class nodeSorter : WebService
    {
        [WebMethod]
        public SortNode GetNodes(int ParentId, string App)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                SortNode parent = new SortNode();
                parent.Id = ParentId;

                ArrayList _nodes = new ArrayList();
                cms.businesslogic.CMSNode n = new cms.businesslogic.CMSNode(ParentId);

                // Root nodes?
                if (ParentId == -1)
                {
                    if (App == "media")
                    {
                        foreach (cms.businesslogic.media.Media child in cms.businesslogic.media.Media.GetRootMedias())
                            _nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                    }
                    else
                        foreach (cms.businesslogic.web.Document child in cms.businesslogic.web.Document.GetRootDocuments())
                            _nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                }
                else
                {
                    // "hack for stylesheet"
                    if (App == "settings")
                    {
                        StyleSheet ss = new StyleSheet(n.Id);
                        foreach (cms.businesslogic.web.StylesheetProperty child in ss.Properties)
                            _nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));

                    }
                    else
                    {
                        //store children array here because iterating over an Array property object is very inneficient.
                        var children = n.Children;
                        foreach (cms.businesslogic.CMSNode child in children)
                        {
                            _nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                        }
                    }
                }

                parent.SortNodes = (SortNode[])_nodes.ToArray(typeof(SortNode));

                return parent;
            }

            throw new ArgumentException("User not logged in");
        }

        [WebMethod]
        public void UpdateSortOrder(int ParentId, string SortOrder)
        {

            try
            {
                if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
                {

                    if (SortOrder.Trim().Length > 0)
                    {
                        string[] tmp = SortOrder.Split(',');

                        bool isContent = false;
                        if (helper.Request("app") == "content" | helper.Request("app") == "")
                            isContent = true;

                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (tmp[i] != "" && tmp[i].Trim() != "")
                            {
                                new cms.businesslogic.CMSNode(int.Parse(tmp[i])).sortOrder = i;

                                if (isContent)
                                {
                                    Document d = new Document(int.Parse(tmp[i]));
                                    // refresh the xml for the sorting to work
                                    if (d.Published)
                                    {
                                        d.refreshXmlSortOrder();
                                        library.UpdateDocumentCache(int.Parse(tmp[i]));
                                    }
                                }
                            }
                        }

                        // Refresh sort order on cached xml
                        if (isContent)
                        {
                            System.Xml.XmlNode parentNode;

                            if (ParentId == -1)
                                parentNode = content.Instance.XmlContent.DocumentElement;
                            else
                                parentNode = content.Instance.XmlContent.GetElementById(ParentId.ToString());

                            //only try to do the content sort if the the parent node is available... 
                            if (parentNode != null)
                                content.SortNodes(ref parentNode);


                            // Load balancing - then refresh entire cache
                            if (UmbracoSettings.UseDistributedCalls)
                                library.RefreshContent();
                        }

                        // fire actionhandler, check for content
                        if ((helper.Request("app") == "content" | helper.Request("app") == "") && ParentId > 0)
                            global::umbraco.BusinessLogic.Actions.Action.RunActionHandlers(new Document(ParentId), ActionSort.Instance);                        
                    }
                }

            }
            catch (Exception ex)
            {
                BusinessLogic.Log.Add(global::umbraco.BusinessLogic.LogTypes.Debug, ParentId, ex.ToString());
            }

        }
    }


    [Serializable]
    public class SortNode
    {
        public SortNode()
        {
        }

        private SortNode[] _sortNodes;

        public SortNode[] SortNodes
        {
            get { return _sortNodes; }
            set { _sortNodes = value; }
        }

        public int TotalNodes
        {
            get { return _sortNodes != null ? _sortNodes.Length : 0; }
            set { int test = value; }
        }


        public SortNode(int Id, int SortOrder, string Name, DateTime CreateDate)
        {
            _id = Id;
            _sortOrder = SortOrder;
            _name = Name;
            _createDate = CreateDate;
        }

        private DateTime _createDate;

        public DateTime CreateDate
        {
            get { return _createDate; }
            set { _createDate = value; }
        }


        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }


        private int _sortOrder;

        public int SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value; }
        }


        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
    }
}