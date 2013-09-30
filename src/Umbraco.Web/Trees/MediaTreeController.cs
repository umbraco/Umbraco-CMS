using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees.Menu;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;
using LegacyMember = umbraco.cms.businesslogic.member.Member;

namespace Umbraco.Web.Trees
{
    [LegacyBaseTree(typeof (loadMembers))]
    [Tree(Constants.Applications.Members, Constants.Trees.Members, "Members")]
    [PluginController("UmbracoTrees")]
    public class MemberTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //list out all the letters
                for (var i = 97; i < 123; i++)
                {
                    var charString = ((char) i).ToString(CultureInfo.InvariantCulture);
                    nodes.Add(CreateTreeNode(charString, queryStrings, charString, "icon-folder-close", true));
                }
                //list out 'Others' if the membership provider is umbraco
                if (LegacyMember.InUmbracoMemberMode())
                {
                    nodes.Add(CreateTreeNode("others", queryStrings, "Others", "icon-folder-close", true));
                }
            }
            else
            {
                //if it is a letter
                if (id.Length == 1 && char.IsLower(id, 0))
                {
                    if (LegacyMember.InUmbracoMemberMode())
                    {
                        //get the members from our member data layer
                        nodes.AddRange(
                            LegacyMember.getMemberFromFirstLetter(id.ToCharArray()[0])
                                        .Select(m => CreateTreeNode(m.LoginName, queryStrings, m.Text, "icon-user")));
                    }
                    else
                    {
                        //get the members from the provider
                        int total;
                        nodes.AddRange(
                            Membership.Provider.FindUsersByName(id + "%", 0, 9999, out total).Cast<MembershipUser>()
                                      .Select(m => CreateTreeNode(m.UserName, queryStrings, m.UserName, "icon-user")));
                    }
                }
                else if (id == "others")
                {
                    //others will only show up when in umbraco membership mode
                    nodes.AddRange(
                        LegacyMember.getAllOtherMembers()
                                    .Select(m => CreateTreeNode(m.Id.ToInvariantString(), queryStrings, m.Text, "icon-user")));
                }
            }
            return nodes;
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                //set default
                menu.DefaultMenuAlias = ActionNew.Instance.Alias;

                // root actions         
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<RefreshNode, ActionRefresh>(true);
                return menu;
            }

            menu.AddMenuItem<ActionDelete>();
            return menu;
        }
    }


    [LegacyBaseTree(typeof(loadMedia))]
    [Tree(Constants.Applications.Media, Constants.Trees.Media, "Media")]
    [PluginController("UmbracoTrees")]
    public class MediaTreeController : ContentTreeControllerBase
    {
        protected override int RecycleBinId
        {
            get { return Constants.System.RecycleBinContent; }
        }

        protected override bool RecycleBinSmells
        {
            get { return Services.MediaService.RecycleBinSmells(); }
        }

        protected override TreeNodeCollection PerformGetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var entities = GetChildEntities(id);

            var nodes = new TreeNodeCollection();            

            nodes.AddRange(
                entities.Cast<UmbracoEntity>()
                        .Select(e => CreateTreeNode(e.Id.ToInvariantString(), queryStrings, e.Name, e.ContentTypeIcon, e.HasChildren)));

            return nodes;

        }

        protected override MenuItemCollection PerformGetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //set the default
            menu.DefaultMenuAlias = ActionNew.Instance.Alias;

            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions         
                menu.AddMenuItem<ActionNew>();
                menu.AddMenuItem<ActionSort>(true);
                menu.AddMenuItem<RefreshNode, ActionRefresh>(true);
                return menu;
            }

            int iid;
            if (int.TryParse(id, out iid) == false)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            var item = Services.EntityService.Get(iid, UmbracoObjectTypes.Media);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            //return a normal node menu:
            menu.AddMenuItem<ActionNew>();
            menu.AddMenuItem<ActionMove>().ConvertLegacyMenuItem(item, "media", "media");
            menu.AddMenuItem<ActionDelete>();
            menu.AddMenuItem<ActionSort>();
            menu.AddMenuItem<ActionRefresh>(true);
            return menu;
        }

        protected override UmbracoObjectTypes UmbracoObjectType
        {
            get { return UmbracoObjectTypes.Media; }
        }
    }
}