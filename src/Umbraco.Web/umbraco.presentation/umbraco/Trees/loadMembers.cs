using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Xml;
using System.Configuration;
using Umbraco.Core.Security;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.contentitem;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.BusinessLogic.Utils;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;


namespace umbraco
{
    /// <summary>
    /// Handles loading of the member application into the application tree
    /// </summary>
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadMembers : BaseTree
    {
        public loadMembers(string application) : base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        /// <summary>
        /// Renders the Javascript.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
                @"
function openMember(id) {
	UmbClientMgr.contentFrame('members/editMember.aspx?id=' + id);
}

function searchMembers(id) {
	UmbClientMgr.contentFrame('members/search.aspx');
}

function viewMembers(letter) {
	UmbClientMgr.contentFrame('members/viewMembers.aspx?letter=' + letter);
}

function openContentItem(id) {
	UmbClientMgr.contentFrame('ContentItem/edit.aspx?id=' + id);
}
");
        }

        /// <summary>
        /// This will call the normal Render method by passing the converted XmlTree to an XmlDocument.
        /// TODO: need to update this render method to do everything that the obsolete render method does and remove the obsolete method
        /// </summary>
        /// <param name="tree"></param>
        public override void Render(ref XmlTree tree)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(tree.ToString(SerializedTreeType.XmlTree));
            Render(ref xDoc);
            tree = SerializableData.Deserialize(xDoc.OuterXml, typeof(XmlTree)) as XmlTree;
			//ensure that the tree type is set! this wouldn't need to be done if BaseTree was implemented properly
			foreach (XmlTreeNode node in tree)
				node.TreeType = this.TreeAlias;
        }

        /// <summary>
        /// Renders the specified tree item.
        /// </summary>
        /// <param name="Tree">The tree.</param>
        public override void Render(ref XmlDocument Tree)
        {
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            string letter = "";
            string ContentItemParent = "";
            if (HttpContext.Current.Request.QueryString.ToString().IndexOf("letter") >= 0)
            {
                letter = HttpContext.Current.Request.QueryString.Get("letter");
            }
            if (HttpContext.Current.Request.QueryString.ToString().IndexOf("ContentItemParent") >= 0)
            {
                ContentItemParent = HttpContext.Current.Request.QueryString.Get("ContentItemParent");
            }
            // letter = ;

            XmlNode root = Tree.DocumentElement;
            if (letter != "")
            {
                if (ContentItemParent != "") // show contentitems owned by the specific member!
                {
                    CMSNode c = new CMSNode(int.Parse(ContentItemParent));
                    var childs = c.ChildrenOfAllObjectTypes;
                    foreach (CMSNode cn in childs)
                    {
                        XmlElement treeElement = Tree.CreateElement("tree");
                        treeElement.SetAttribute("menu", "D,L");
                        treeElement.SetAttribute("nodeID", cn.Id.ToString());
                        treeElement.SetAttribute("text", cn.Text);
                        // treeElement.SetAttribute("action", "javascript:openMember(" + m.Id.ToString() + ");");
                        treeElement.SetAttribute("action", "javascript:openContentItem(" + cn.Id + ");");
                        if (!cn.HasChildren)
                        {
                            treeElement.SetAttribute("src", "");
                        }
                        else
                        {
                            treeElement.SetAttribute("src",
                                "tree.aspx?letter=" + letter + "&app=" + m_app + "&treeType=" +
                                HttpContext.Current.Request.QueryString["treeType"] + "&ContentItemParent=" + cn.Id + "&rnd=" + Guid.NewGuid());
                        }
                        treeElement.SetAttribute("icon", "doc.gif");
                        treeElement.SetAttribute("openIcon", "doc.gif");
                        treeElement.SetAttribute("nodeType", "contentItem");
                        treeElement.SetAttribute("hasChildren", "true");
                        root.AppendChild(treeElement);
                    }
                }
                else // list all members with selected first character.
                {
                    //if letters equals Others show members that not starts with a through z
                    if (letter.Equals("Others"))
                    {
                        foreach (Member m in Member.getAllOtherMembers())
                        {
                            XmlElement treeElement = Tree.CreateElement("tree");

                            treeElement.SetAttribute("nodeID", m.LoginName);
                            treeElement.SetAttribute("text", m.Text);
                            treeElement.SetAttribute("action", "javascript:openMember('" + m.Id + "');");
                            treeElement.SetAttribute("menu", "D");
                            treeElement.SetAttribute("icon", string.IsNullOrEmpty(m.ContentType.IconUrl) ? "member.gif" : m.ContentType.IconUrl);
                            treeElement.SetAttribute("openIcon", string.IsNullOrEmpty(m.ContentType.IconUrl) ? "member.gif" : m.ContentType.IconUrl);
                            treeElement.SetAttribute("nodeType", "member");
                            treeElement.SetAttribute("hasChildren", "true");
                            root.AppendChild(treeElement);
                        }
                    }
                    else
                    {
                        if (provider.IsUmbracoMembershipProvider())
                        {
                            foreach (Member m in Member.getMemberFromFirstLetter(letter.ToCharArray()[0]))
                            {
                                XmlElement treeElement = Tree.CreateElement("tree");

                                treeElement.SetAttribute("nodeID", m.LoginName);
                                treeElement.SetAttribute("text", m.Text);
                                treeElement.SetAttribute("action", "javascript:openMember('" + m.Id + "');");
                                treeElement.SetAttribute("menu", "D");
                                treeElement.SetAttribute("icon", string.IsNullOrEmpty(m.ContentType.IconUrl) ? "member.gif" : m.ContentType.IconUrl);
                                treeElement.SetAttribute("openIcon", string.IsNullOrEmpty(m.ContentType.IconUrl) ? "member.gif" : m.ContentType.IconUrl);
                                treeElement.SetAttribute("nodeType", "member");
                                treeElement.SetAttribute("hasChildren", "true");
                                root.AppendChild(treeElement);
                            }
                        }
                        else
                        {
                            int total;
                            foreach (MembershipUser u in provider.FindUsersByName(letter + "%", 0, 9999, out total))
                            {
                                XmlElement treeElement = Tree.CreateElement("tree");

                                treeElement.SetAttribute("nodeID", u.UserName);
                                treeElement.SetAttribute("text", u.UserName);
                                treeElement.SetAttribute("action", "javascript:openMember('" + HttpContext.Current.Server.UrlEncode(u.UserName) + "');");
                                treeElement.SetAttribute("menu", "D");
                                treeElement.SetAttribute("icon", "member.gif");
                                treeElement.SetAttribute("openIcon", "member.gif");
                                treeElement.SetAttribute("nodeType", "member");
                                treeElement.SetAttribute("hasChildren", "true");
                                root.AppendChild(treeElement);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 97; i < 123; i++)
                {
                    XmlElement treeElement = Tree.CreateElement("tree");
                    treeElement.SetAttribute("menu", "");
                    treeElement.SetAttribute("nodeID", i.ToString());
                    treeElement.SetAttribute("text", ((char)i).ToString());
                    treeElement.SetAttribute("action", "javascript:viewMembers('" + ((char)i).ToString() + "');");

                    treeElement.SetAttribute("src", "");

                    treeElement.SetAttribute("icon", FolderIcon);
                    treeElement.SetAttribute("openIcon", FolderIcon);
                    treeElement.SetAttribute("nodeType", "member");
                    treeElement.SetAttribute("hasChildren", "true");

                    treeElement.SetAttribute("src",
                        "tree.aspx?letter=" + ((char)i) + "&app=" + m_app + "&treeType=" +
                        HttpContext.Current.Request.QueryString["treeType"] + "&rnd=" + Guid.NewGuid());


                    root.AppendChild(treeElement);
                }

                //Add folder named "Others", only supported by umbraco
                if (provider.IsUmbracoMembershipProvider())
                {
                    XmlElement treeElementOther = Tree.CreateElement("tree");
                    treeElementOther.SetAttribute("menu", "");
                    treeElementOther.SetAttribute("nodeID", "Others");
                    treeElementOther.SetAttribute("text", "Others");
                    treeElementOther.SetAttribute("action", "javascript:viewMembers('#');");
                    treeElementOther.SetAttribute("src", "");
                    treeElementOther.SetAttribute("icon", FolderIcon);
                    treeElementOther.SetAttribute("openIcon", FolderIcon);
                    treeElementOther.SetAttribute("nodeType", "member");
                    treeElementOther.SetAttribute("hasChildren", "true");

                    treeElementOther.SetAttribute("src", "tree.aspx?letter=Others&app=" + m_app + "&treeType=" +
                        HttpContext.Current.Request.QueryString["treeType"] + "&rnd=" +
                        Guid.NewGuid());

                    root.AppendChild(treeElementOther);
                }

                // Search
                XmlElement treeElementSearch = Tree.CreateElement("tree");
                treeElementSearch.SetAttribute("menu", "");
                treeElementSearch.SetAttribute("nodeID", "Search");
                treeElementSearch.SetAttribute("text", ui.Text("search"));
                treeElementSearch.SetAttribute("action", "javascript:searchMembers();");
                treeElementSearch.SetAttribute("src", "");
                treeElementSearch.SetAttribute("icon", FolderIcon);
                treeElementSearch.SetAttribute("openIcon", FolderIcon);
                treeElementSearch.SetAttribute("nodeType", "member");


                root.AppendChild(treeElementSearch);

            }
        }
    }


}
