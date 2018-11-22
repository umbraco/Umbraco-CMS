using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Web;
using Umbraco.Web.WebServices;
using umbraco.BasePages;
using umbraco.BusinessLogic;
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
    public class nodeSorter : UmbracoAuthorizedWebService
    {
        [WebMethod]
        public SortNode GetNodes(string ParentId, string App)
        {
            if (BasePage.ValidateUserContextID(BasePage.umbracoUserContextID))
            {
                var nodes = new List<SortNode>();

                // "hack for stylesheet"
                if (App == "settings")
                {
                    var stylesheet = Services.FileService.GetStylesheetByName(ParentId.EnsureEndsWith(".css"));
                    if (stylesheet == null) throw new InvalidOperationException("No stylesheet found by name " + ParentId);

                    var sort = 0;
                    foreach (var child in stylesheet.Properties)
                    {
                        nodes.Add(new SortNode(child.Name.GetHashCode(), sort, child.Name, DateTime.Now));
                        sort++;
                    }

                    return new SortNode()
                    {
                        SortNodes = nodes.ToArray()
                    };
                }
                else
                {
                    var asInt = int.Parse(ParentId);

                    var parent = new SortNode { Id = asInt };

                    var entityService = base.ApplicationContext.Services.EntityService;

                    // Root nodes?
                    if (asInt == -1)
                    {
                        if (App == "media")
                        {
                            var rootMedia = entityService.GetRootEntities(UmbracoObjectTypes.Media);
                            nodes.AddRange(rootMedia.Select(media => new SortNode(media.Id, media.SortOrder, media.Name, media.CreateDate)));
                        }
                        else
                        {
                            var rootContent = entityService.GetRootEntities(UmbracoObjectTypes.Document);
                            nodes.AddRange(rootContent.Select(content => new SortNode(content.Id, content.SortOrder, content.Name, content.CreateDate)));
                        }
                    }
                    else
                    {
                        var children = entityService.GetChildren(asInt);
                        nodes.AddRange(children.Select(child => new SortNode(child.Id, child.SortOrder, child.Name, child.CreateDate)));
                    }


                    parent.SortNodes = nodes.ToArray();

                    return parent;
                }
            }

            throw new ArgumentException("User not logged in");
        }

        public void UpdateSortOrder(int ParentId, string SortOrder)
        {
            UpdateSortOrder(ParentId.ToString(), SortOrder);
        }

        [WebMethod]
        public void UpdateSortOrder(string ParentId, string SortOrder)
        {
            if (AuthorizeRequest() == false) return;
            if (SortOrder.Trim().Length <= 0) return;

            var isContent = helper.Request("app") == "content" | helper.Request("app") == "";
            var isMedia = helper.Request("app") == "media";

            //ensure user is authorized for the app requested
            if (isContent && AuthorizeRequest(DefaultApps.content.ToString()) == false) return;
            if (isMedia && AuthorizeRequest(DefaultApps.media.ToString()) == false) return;

            var ids = SortOrder.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (isContent)
            {
                SortContent(ids, int.Parse(ParentId));
            }
            else if (isMedia)
            {
                SortMedia(ids);
            }
            else
            {
                SortStylesheetProperties(ParentId, ids);
            }
        }

        private void SortMedia(string[] ids)
        {
            var mediaService = base.ApplicationContext.Services.MediaService;
            var sortedMedia = new List<IMedia>();
            try
            {
                for (var i = 0; i < ids.Length; i++)
                {
                    var id = int.Parse(ids[i]);
                    var m = mediaService.GetById(id);
                    sortedMedia.Add(m);
                }

                // Save Media with new sort order and update content xml in db accordingly
                var sorted = mediaService.Sort(sortedMedia);
            }
            catch (Exception ex)
            {
                LogHelper.Error<nodeSorter>("Could not update media sort order", ex);
            }
        }


        private void SortStylesheetProperties(string stylesheetName, string[] names)
        {
            var stylesheet = Services.FileService.GetStylesheetByName(stylesheetName.EnsureEndsWith(".css"));
            if (stylesheet == null) throw new InvalidOperationException("No stylesheet found by name " + stylesheetName);

            var currProps = stylesheet.Properties.ToArray();
            //remove them all first
            foreach (var prop in currProps)
            {
                stylesheet.RemoveProperty(prop.Name);
            }

            //re-add them in the right order
            for (var i = 0; i < names.Length; i++)
            {
                var found = currProps.Single(x => x.Name == names[i]);
                stylesheet.AddProperty(found);
            }

            Services.FileService.SaveStylesheet(stylesheet);
        }

        private void SortContent(string[] ids, int parentId)
        {
            var contentService = ApplicationContext.Services.ContentService;
            try
            {
                // Save content with new sort order and update db+cache accordingly
                var intIds = new List<int>();
                foreach (var stringId in ids)
                {
                    int intId;
                    if (int.TryParse(stringId, out intId))
                        intIds.Add(intId);
                }
                var sorted = contentService.Sort(intIds.ToArray());

                // refresh sort order on cached xml
                // but no... this is not distributed - solely relying on content service & events should be enough
                //content.Instance.SortNodes(parentId);

                //send notifications! TODO: This should be put somewhere centralized instead of hard coded directly here
                if (parentId > 0)
                {
                    ApplicationContext.Services.NotificationService.SendNotification(contentService.GetById(parentId), ActionSort.Instance, UmbracoContext, ApplicationContext);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error<nodeSorter>("Could not update content sort order", ex);
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