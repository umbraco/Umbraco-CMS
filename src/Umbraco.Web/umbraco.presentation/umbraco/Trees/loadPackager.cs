using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Configuration;
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
using Umbraco.Core;
using umbraco.cms.presentation.Trees;

namespace umbraco
{
    /// <summary>
    /// Handles loading of the packager application into the developer application tree
    /// </summary>
    [Tree(Constants.Applications.Developer, "packager", "Packages", sortOrder: 3)]
    public class loadPackager : BaseTree
    {
        #region TreeI Members
        public loadPackager(string application) : base(application) { }
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            
        }

        private int _id;
        private string _app;

        /// <summary>
        /// Sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int id
        {
            set { _id = value; }
        }

        /// <summary>
        /// Sets the app.
        /// </summary>
        /// <value>The app.</value>
        public string app
        {
            set { _app = value; }
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(umbraco.BusinessLogic.Actions.ActionRefresh.Instance);
        }

        protected override void CreateRootNodeActions(ref List<IAction> actions)
        {
            actions.Clear();

            // U4-4422 : There is no variable nodes on this so no need to reload nodes
            //actions.Add(umbraco.BusinessLogic.Actions.ActionRefresh.Instance);
        }

        /// <summary>
        /// Renders the Javascript.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
            @"function openPackageCategory(url) {
			UmbClientMgr.contentFrame('developer/packages/' + url);}"
            );
        }

        /// <summary>
        /// Renders the specified tree item.
        /// </summary>
        /// <param name="tree">The tree.</param>
        public override void Render(ref XmlTree tree)
        {            
            string[,] items = { { "BrowseRepository.aspx", "Install from repository" }, { "CreatePackage.aspx", "Created Packages" }, { "installedPackages.aspx", "Installed packages" }, { "StarterKits.aspx", "Starter kit" }, { "installer.aspx", "Install local package" } };


            for (int i = 0; i <= items.GetUpperBound(0); i++)
            {
                XmlTreeNode xNode = XmlTreeNode.Create(this);
                xNode.NodeID = (i + 1).ToInvariantString();
                xNode.Text = items[i, 1];
                xNode.Icon = "icon-folder";
                xNode.OpenIcon = "icon-folder";
                
                
                //Make sure the different sections load the correct childnodes.
                switch (items[i, 0])
                {
                    case "installedPackages.aspx":
                        
                        if (cms.businesslogic.packager.InstalledPackage.GetAllInstalledPackages().Count > 0)
                        {
                            xNode.Source = "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=installed" + "&rnd=" + Guid.NewGuid();
                            xNode.NodeType = "installedPackages";                            
                            xNode.Text =  ui.Text("treeHeaders", "installedPackages");
                            xNode.HasChildren = true;
                        }
                        else
                        {
                            xNode.Text = "";
                        }

                        xNode.Action = "javascript:void(0);";

                        break;


                    case "BrowseRepository.aspx":

                        /*
                        //Gets all the repositories registered in umbracoSettings.config
                        var repos = cms.businesslogic.packager.repositories.Repository.getAll();
                        

                        //if more then one repo, then list them as child nodes under the "Install from repository" node.
                        // the repositories will then be fetched from the loadPackages class.
                        if (repos.Count > 1)
                        {
                            xNode.Source = "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=repositories" + "&rnd=" + Guid.NewGuid();
                            xNode.NodeType = "packagesRepositories";                            
                            xNode.Text = ui.Text("treeHeaders", "repositories");
                            xNode.HasChildren = true;
                        }
                        */
                        //if only one repo, then just list it directly and name it as the repository.
                        //the packages will be loaded from the loadPackages class with a repoAlias querystring
                        var repos = cms.businesslogic.packager.repositories.Repository.getAll();
                        
                        xNode.Text = repos[0].Name;
                        xNode.Source = "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repos[0].Guid + "&rnd=" + Guid.NewGuid();
                        xNode.NodeType = "packagesRepository";
                        xNode.Action = "javascript:openPackageCategory('BrowseRepository.aspx?repoGuid=" + repos[0].Guid + "');";
                        xNode.Icon = "icon-server-alt";
                        xNode.HasChildren = true;

                        break;


                    case "CreatePackage.aspx":
                        xNode.Source = "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=created" + "&rnd=" + Guid.NewGuid();
                        xNode.NodeType = "createdPackages";
                        xNode.Menu.Clear();                        
                        xNode.Menu.Add(umbraco.BusinessLogic.Actions.ActionNew.Instance);
                        xNode.Menu.Add(umbraco.BusinessLogic.Actions.ActionRefresh.Instance);
                        xNode.Text = ui.Text("treeHeaders", "createdPackages");
                        xNode.HasChildren = true;
                        xNode.Action = "javascript:void(0);";
                        
                        break;

                    case "installer.aspx":
                        xNode.Source = "";
                        xNode.NodeType = "uploadPackage";
                        xNode.Icon = "icon-page-up";
                        xNode.Action = "javascript:openPackageCategory('" + items[i, 0] + "');";
                        xNode.Text = ui.Text("treeHeaders", "localPackage");
                        xNode.Menu.Clear();
                        break;

                    case "StarterKits.aspx":
                        xNode.Source = "";
                        xNode.NodeType = "starterKits";
                        xNode.Action = "javascript:openPackageCategory('" + items[i, 0] + "');";
                        xNode.Icon = "icon-flash";
                        xNode.Text = ui.Text("treeHeaders", "installStarterKit");
                        xNode.Menu.Clear();
                        break;

                    default:
                        break;
                }

                if (xNode.Text != "")
                    tree.Add(xNode);
            }

        }
    }

        #endregion
    
}
