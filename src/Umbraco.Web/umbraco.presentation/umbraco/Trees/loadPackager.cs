using System;
using System.Collections.Generic;
using System.Text;
using umbraco.interfaces;
using Umbraco.Core;
using umbraco.cms.presentation.Trees;

namespace umbraco
{
    /// <summary>
    /// Handles loading of the packager application into the developer application tree
    /// </summary>
    //[Tree(Constants.Applications.Developer, "packager", "Packages", sortOrder: 3)]
    [Obsolete("This is no longer used and will be removed from the codebase in the future")]
    public class loadPackager : BaseTree
    {
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
            actions.Add(BusinessLogic.Actions.ActionRefresh.Instance);
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

            for (var i = 0; i <= items.GetUpperBound(0); i++)
            {
                var xNode = XmlTreeNode.Create(this);
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
                            xNode.Source = $"tree.aspx?app={_app}&id={_id}&treeType=packagerPackages&packageType=installed&rnd={Guid.NewGuid()}";
                            xNode.NodeType = "installedPackages";
                            xNode.Text = ui.Text("treeHeaders", "installedPackages");
                            xNode.HasChildren = true;
                        }
                        else
                        {
                            xNode.Text = "";
                        }
                        xNode.Action = "javascript:void(0);";
                        break;


                    case "BrowseRepository.aspx":
                        xNode.Text = Constants.PackageRepository.DefaultRepositoryName;
                        xNode.Source = $"tree.aspx?app={_app}&id={_id}&treeType=packagerPackages&packageType=repository&repoGuid={Constants.PackageRepository.DefaultRepositoryId}&rnd={Guid.NewGuid()}";
                        xNode.NodeType = "packagesRepository";
                        xNode.Action = $"javascript:openPackageCategory(\'BrowseRepository.aspx?repoGuid={Constants.PackageRepository.DefaultRepositoryId}\');";
                        xNode.Icon = "icon-server-alt";
                        xNode.HasChildren = true;
                        break;

                    case "CreatePackage.aspx":
                        xNode.Source = $"tree.aspx?app={_app}&id={_id}&treeType=packagerPackages&packageType=created&rnd={Guid.NewGuid()}";
                        xNode.NodeType = "createdPackages";
                        xNode.Menu.Clear();
                        xNode.Menu.Add(BusinessLogic.Actions.ActionNew.Instance);
                        xNode.Menu.Add(BusinessLogic.Actions.ActionRefresh.Instance);
                        xNode.Text = ui.Text("treeHeaders", "createdPackages");
                        xNode.HasChildren = true;
                        xNode.Action = "javascript:void(0);";
                        break;

                    case "installer.aspx":
                        xNode.Source = "";
                        xNode.NodeType = "uploadPackage";
                        xNode.Icon = "icon-page-up";
                        xNode.Action = $"javascript:openPackageCategory(\'{items[i, 0]}\');";
                        xNode.Text = ui.Text("treeHeaders", "localPackage");
                        xNode.Menu.Clear();
                        break;

                    case "StarterKits.aspx":
                        xNode.Source = "";
                        xNode.NodeType = "starterKits";
                        xNode.Action = $"javascript:openPackageCategory(\'{items[i, 0]}\');";
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
}
