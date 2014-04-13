using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using umbraco.interfaces;

namespace umbraco
{
    [Tree(Constants.Applications.Developer, "packagerPackages", "Packager Packages", initialize: false, sortOrder: 1)]
    public class loadPackages : BaseTree
    {

        public const string PACKAGE_TREE_PREFIX = "package_";

        public loadPackages(string application) : base(application) { }
        
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            
        }
        
        private int _id;
        private string _app;
        private string _packageType = "";
        private string _repoGuid = "";

        public override void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(@"
            function openCreatedPackage(id) {
	            UmbClientMgr.contentFrame('developer/packages/editPackage.aspx?id=' + id);
            }
            function openInstalledPackage(id) {
	            UmbClientMgr.contentFrame('developer/packages/installedPackage.aspx?id=' + id);
            }
            ");
        }

        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
        }

        public override void Render(ref XmlTree tree)
        {

            _packageType = HttpContext.Current.Request.QueryString["packageType"];
           
            switch (_packageType)
            {
                case "installed":
                    Version v;
                    // Display the unique packages, ordered by the latest version number. [LK 2013-06-10]
                    var uniquePackages = InstalledPackage.GetAllInstalledPackages()
                        .OrderByDescending(x => Version.TryParse(x.Data.Version, out v) ? v : new Version())
                        .GroupBy(x => x.Data.Name)
                        .Select(x => x.First())
                        .OrderBy(x => x.Data.Name);
                    foreach (var p in uniquePackages)
                    {
                        var xNode = XmlTreeNode.Create(this);
                        xNode.NodeID = string.Concat(PACKAGE_TREE_PREFIX, p.Data.Id);
                        xNode.Text = p.Data.Name;
                        xNode.Action = string.Format("javascript:openInstalledPackage('{0}');", p.Data.Id);
                        xNode.Icon = "icon-box";
                        xNode.OpenIcon = "icon-box";
                        xNode.NodeType = "createdPackageInstance";
                        tree.Add(xNode);
                    }
                    break;

                case "created":
                    foreach (CreatedPackage p in CreatedPackage.GetAllCreatedPackages())
                    {

                        XmlTreeNode xNode = XmlTreeNode.Create(this);
                        xNode.NodeID = PACKAGE_TREE_PREFIX + p.Data.Id.ToString();
                        xNode.Text = p.Data.Name;
                        xNode.Action = "javascript:openCreatedPackage('" + p.Data.Id.ToString() + "');";
                        xNode.Icon = "icon-box";
                        xNode.OpenIcon = "icon-box";
                        xNode.NodeType = "createdPackageInstance";
                        xNode.Menu.Add(umbraco.BusinessLogic.Actions.ActionDelete.Instance);
                        tree.Add(xNode);
                    }
                    break;

                case "repositories":
                    List<cms.businesslogic.packager.repositories.Repository> repos = cms.businesslogic.packager.repositories.Repository.getAll();

                    foreach (cms.businesslogic.packager.repositories.Repository repo in repos)
                    {
                        XmlTreeNode xNode = XmlTreeNode.Create(this);
                        xNode.Text = repo.Name;
                        xNode.Action = "javascript:openPackageCategory('BrowseRepository.aspx?repoGuid=" + repo.Guid + "');";
                        xNode.Icon = "icon-server-alt";
                        xNode.OpenIcon = "icon-server-alt";
                        xNode.NodeType = "packagesRepo" + repo.Guid;
                        xNode.Menu.Add( umbraco.BusinessLogic.Actions.ActionRefresh.Instance );
                        xNode.Source = "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repo.Guid + "&rnd=" + Guid.NewGuid();
                        tree.Add(xNode);
                        
                    }

                    break;
                case "repository":

                    _repoGuid = HttpContext.Current.Request.QueryString["repoGuid"];
                    Umbraco.Web.org.umbraco.our.Repository r = new Umbraco.Web.org.umbraco.our.Repository();
                    
                    foreach (var cat in r.Categories(_repoGuid))
                    {
                        XmlTreeNode xNode = XmlTreeNode.Create(this);
                        xNode.NodeID = cat.Id.ToInvariantString();
                        xNode.Text = cat.Text;
                        xNode.Action = "javascript:openPackageCategory('BrowseRepository.aspx?category=" + cat.Id + "&repoGuid=" + _repoGuid + "');";
                        xNode.Icon = "icon-folder";
                        xNode.OpenIcon = "icon-folder";
                        xNode.NodeType = "packagesCategory" + cat.Id;                        
                        tree.Add(xNode);
                    
                    }
                    
                    break;
            }

        }

        
    }

}
