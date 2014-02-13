﻿using System;
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
        
        private int m_id;
        private string m_app;
        private string m_packageType = "";
        private string m_repoGuid = "";

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

        public override void Render(ref XmlTree tree)
        {

            m_packageType = HttpContext.Current.Request.QueryString["packageType"];
           
            switch (m_packageType)
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
                        xNode.Icon = "package.gif";
                        xNode.OpenIcon = "package.gif";
                        xNode.NodeType = "createdPackageInstance";
                        xNode.Menu = null;
                        tree.Add(xNode);
                    }
                    break;

                case "created":
                    foreach (cms.businesslogic.packager.CreatedPackage p in cms.businesslogic.packager.CreatedPackage.GetAllCreatedPackages())
                    {

                        XmlTreeNode xNode = XmlTreeNode.Create(this);
                        xNode.NodeID = PACKAGE_TREE_PREFIX + p.Data.Id.ToString();
                        xNode.Text = p.Data.Name;
                        xNode.Action = "javascript:openCreatedPackage('" + p.Data.Id.ToString() + "');";
                        xNode.Icon = "package.gif";
                        xNode.OpenIcon = "package.gif";
                        xNode.NodeType = "createdPackageInstance";
//                        xNode.Menu.Add( umbraco.BusinessLogic.Actions.ActionDelete.Instance );

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
                        xNode.Icon = "repository.gif";
                        xNode.OpenIcon = "repository.gif";
                        xNode.NodeType = "packagesRepo" + repo.Guid;
                        xNode.Menu.Add( umbraco.BusinessLogic.Actions.ActionRefresh.Instance );
                        xNode.Source = "tree.aspx?app=" + this.m_app + "&id=" + this.m_id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repo.Guid + "&rnd=" + Guid.NewGuid();
                        tree.Add(xNode);
                        /*
                        XmlElement catElement = Tree.CreateElement("tree");
                        catElement.SetAttribute("text", repo.Name);
                        catElement.SetAttribute("menu", "L");

                        catElement.SetAttribute("icon", "repository.gif");
                        catElement.SetAttribute("openIcon", "repository.gif");

                        catElement.SetAttribute("nodeType", "packagesRepo" + repo.Guid);
                        catElement.SetAttribute("src", "tree.aspx?app=" + this.m_app + "&id=" + this.m_id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repo.Guid + "&rnd=" + Guid.NewGuid());
                        catElement.SetAttribute("action", "javascript:openPackageCategory('BrowseRepository.aspx?repoGuid=" + repo.Guid + "');");
                        root.AppendChild(catElement);
                         * */
                    }

                    break;
                case "repository":

                    m_repoGuid = HttpContext.Current.Request.QueryString["repoGuid"];
                    cms.businesslogic.packager.repositories.Repository currentRepo = cms.businesslogic.packager.repositories.Repository.getByGuid(m_repoGuid);
                    if (currentRepo != null)
                    {

                        foreach (cms.businesslogic.packager.repositories.Category cat in currentRepo.Webservice.Categories(currentRepo.Guid))
                        {

                            XmlTreeNode xNode = XmlTreeNode.Create(this);
                            xNode.Text = cat.Text;
                            xNode.Action = "javascript:openPackageCategory('BrowseRepository.aspx?category=" + cat.Id + "&repoGuid=" + currentRepo.Guid + "');";
                            xNode.Icon = "folder.gif";
                            xNode.OpenIcon = "folder.gif";
                            xNode.NodeType = "packagesCategory" + cat.Id;                        
                            tree.Add(xNode);
                            /*
                            XmlElement catElement = Tree.CreateElement("tree");
                            catElement.SetAttribute("text", cat.Text);
                            //catElement.SetAttribute("menu", "");
                            catElement.SetAttribute("icon", "folder.gif");
                            catElement.SetAttribute("openIcon", "folder_o.gif");
                            catElement.SetAttribute("nodeType", "packagesCategory" + cat.Id);
                            catElement.SetAttribute("action", "javascript:openPackageCategory('BrowseRepository.aspx?category=" + cat.Id + "&repoGuid=" + currentRepo.Guid + "');");
                            root.AppendChild(catElement);*/
                        }
                    }
                    break;
            }

        }

        
    }

    public class _loadPackages : ITree
    {

        private int m_id;
        private string m_app;
        private string m_packageType = "";
        private string m_repoGuid = "";

        int ITree.id
        {
            set { m_id = value; }
        }

        string ITree.app
        {
            set { m_app = value; }
        }

        void ITree.Render(ref XmlDocument Tree)
        {
            m_packageType = HttpContext.Current.Request.QueryString["packageType"];
            XmlNode root = Tree.DocumentElement;

            switch (m_packageType)
            {
                case "installed":
                    foreach (cms.businesslogic.packager.InstalledPackage p in cms.businesslogic.packager.InstalledPackage.GetAllInstalledPackages())
                    {
                        XmlElement treeElement = Tree.CreateElement("tree");
                        treeElement.SetAttribute("nodeID", "package_" + p.Data.Id.ToString());
                        treeElement.SetAttribute("text", p.Data.Name);
                        treeElement.SetAttribute("action", "javascript:openInstalledPackage('" + p.Data.Id.ToString() + "');");
                        treeElement.SetAttribute("menu", "");
                        //treeElement.SetAttribute("src", "");
                        treeElement.SetAttribute("icon", "package.gif");
                        treeElement.SetAttribute("openIcon", "package.gif");
                        treeElement.SetAttribute("nodeType", "createdPackageInstance");
                        root.AppendChild(treeElement);
                    }
                    break;

                case "created":
                    foreach (cms.businesslogic.packager.CreatedPackage p in cms.businesslogic.packager.CreatedPackage.GetAllCreatedPackages())
                    {
                        XmlElement treeElement = Tree.CreateElement("tree");
                        treeElement.SetAttribute("nodeID", "package_" + p.Data.Id.ToString());
                        treeElement.SetAttribute("text", p.Data.Name);
                        treeElement.SetAttribute("action", "javascript:openCreatedPackage('" + p.Data.Id.ToString() + "');");
                        treeElement.SetAttribute("menu", "D");
                        treeElement.SetAttribute("src", "");
                        treeElement.SetAttribute("icon", "package.gif");
                        treeElement.SetAttribute("openIcon", "package.gif");
                        treeElement.SetAttribute("nodeType", "createdPackageInstance");
                        root.AppendChild(treeElement);
                    }
                    break;

                case "repositories":
                    List<cms.businesslogic.packager.repositories.Repository> repos = cms.businesslogic.packager.repositories.Repository.getAll();

                    foreach (cms.businesslogic.packager.repositories.Repository repo in repos)
                    {
                        XmlElement catElement = Tree.CreateElement("tree");
                        catElement.SetAttribute("text", repo.Name);
                        catElement.SetAttribute("menu", "L");

                        catElement.SetAttribute("icon", "repository.gif");
                        catElement.SetAttribute("openIcon", "repository.gif");

                        catElement.SetAttribute("nodeType", "packagesRepo" + repo.Guid);
                        catElement.SetAttribute("src", "tree.aspx?app=" + this.m_app + "&id=" + this.m_id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repo.Guid + "&rnd=" + Guid.NewGuid());
                        catElement.SetAttribute("action", "javascript:openPackageCategory('BrowseRepository.aspx?repoGuid=" + repo.Guid + "');");
                        root.AppendChild(catElement);
                    }

                    break;
                case "repository":

                    m_repoGuid = HttpContext.Current.Request.QueryString["repoGuid"];
                    cms.businesslogic.packager.repositories.Repository currentRepo = cms.businesslogic.packager.repositories.Repository.getByGuid(m_repoGuid);
                    if (currentRepo != null)
                    {

                        foreach (cms.businesslogic.packager.repositories.Category cat in currentRepo.Webservice.Categories(currentRepo.Guid))
                        {
                            XmlElement catElement = Tree.CreateElement("tree");
                            catElement.SetAttribute("text", cat.Text);
                            //catElement.SetAttribute("menu", "");
                            catElement.SetAttribute("icon", "folder.gif");
                            catElement.SetAttribute("openIcon", "folder_o.gif");
                            catElement.SetAttribute("nodeType", "packagesCategory" + cat.Id);
                            catElement.SetAttribute("action", "javascript:openPackageCategory('BrowseRepository.aspx?category=" + cat.Id + "&repoGuid=" + currentRepo.Guid + "');");
                            root.AppendChild(catElement);
                        }
                    }
                    break;
            }

            //SD: Commented this out... not sure why it was here??
            //throw new Exception("The method or operation is not implemented.");
        }

        public void RenderJS(ref System.Text.StringBuilder Javascript)
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

    }
    
}
