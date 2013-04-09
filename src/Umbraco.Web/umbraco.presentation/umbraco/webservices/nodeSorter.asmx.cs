using System;
using System.Collections;
using System.ComponentModel;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Caching;
using Umbraco.Web;
using Umbraco.Web.WebServices;
using Umbraco.Web.Security;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

namespace umbraco.presentation.webservices
{
    /// <summary>
    /// Summary description for nodeSorter
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    [ScriptService]
    public class nodeSorter : UmbracoAuthorizedWebService
    {
        [WebMethod]
        public SortNode GetNodes(int ParentId, string App)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                var parent = new SortNode { Id = ParentId };

                var nodes = new ArrayList();
                var cmsNode = new cms.businesslogic.CMSNode(ParentId);

                // Root nodes?
                if (ParentId == -1)
                {
                    if (App == Constants.Applications.Media)
                    {
                        foreach (cms.businesslogic.media.Media child in cms.businesslogic.media.Media.GetRootMedias())
                            nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                    }
                    else
                        foreach (Document child in Document.GetRootDocuments())
                            nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                }
                else
                {
                    // "hack for stylesheet"
                    if (App == Constants.Applications.Settings)
                    {
                        var styleSheet = new StyleSheet(cmsNode.Id);
                        foreach (var child in styleSheet.Properties)
                            nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));

                    }
                    else
                    {
                        //store children array here because iterating over an Array property object is very inneficient.
                        var children = cmsNode.Children;
                        foreach (cms.businesslogic.CMSNode child in children)
                            nodes.Add(new SortNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime));
                    }
                }

                parent.SortNodes = (SortNode[])nodes.ToArray(typeof(SortNode));

                return parent;
            }

            throw new ArgumentException("User not logged in");
        }

        [WebMethod]
        public void UpdateSortOrder(int ParentId, string SortOrder)
        {
            try
            {
                if (!AuthorizeRequest()) return;
                if (SortOrder.Trim().Length <= 0) return;
                var tmp = SortOrder.Split(',');

                var isContent = Context.Request.GetItemAsString("app") == Constants.Applications.Content | helper.Request("app") == "";
                var isMedia = Context.Request.GetItemAsString("app") == Constants.Applications.Media;

                //ensure user is authorized for the app requested
                if (isContent && !AuthorizeRequest(DefaultApps.content.ToString())) return;
                if (isMedia && !AuthorizeRequest(DefaultApps.media.ToString())) return;

                for (var i = 0; i < tmp.Length; i++)
                {
                    if (tmp[i] == "" || tmp[i].Trim() == "") continue;
                    
                    if (isContent)
                    {
                        var document = new Document(int.Parse(tmp[i]));
                        var published = document.Published;
                        document.sortOrder = i;
                        document.Save();
                        // refresh the xml for the sorting to work
                        if (published)
                        {
                            document.SaveAndPublish(UmbracoUser);
                            document.refreshXmlSortOrder();
                        }
                    }
                    // to update the sortorder of the media node in the XML, re-save the node....
                    else if (isMedia)
                    {
                        var media = new cms.businesslogic.media.Media(int.Parse(tmp[i]));
                        media.sortOrder = i;
                        media.Save();
                    }
                    else
                    {
                        new cms.businesslogic.CMSNode(int.Parse(tmp[i])).sortOrder = i;
                    }
                }

                // Refresh sort order on cached xml
                if (isContent)
                {
                    XmlNode parentNode = ParentId == -1
                                             ? content.Instance.XmlContent.DocumentElement
                                             : content.Instance.XmlContent.GetElementById(ParentId.ToString());

                    //only try to do the content sort if the the parent node is available... 
                    if (parentNode != null)
                        content.SortNodes(ref parentNode);

                    // Load balancing - then refresh entire cache
                            // NOTE: SD: This seems a bit excessive to do simply for sorting! I'm going to leave this here for now but 
                            //  the sort order should be updated in distributed calls when an item is Published (and it most likely is)
                            //  but I guess this was put here for a reason at some point.
                    if (UmbracoSettings.UseDistributedCalls)
                        library.RefreshContent();
                }

                // fire actionhandler, check for content
                if ((helper.Request("app") == Constants.Applications.Content | helper.Request("app") == "") && ParentId > 0)
                    BusinessLogic.Actions.Action.RunActionHandlers(new Document(ParentId), ActionSort.Instance);
            }
            catch (Exception ex)
            {
                LogHelper.Error<nodeSorter>("Could not update sort order", ex);
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