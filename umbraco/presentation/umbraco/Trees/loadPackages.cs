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

namespace umbraco
{
    public class loadPackages : ITree
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
	            parent.right.document.location.href = 'developer/packages/editPackage.aspx?id=' + id;
            }
            function openInstalledPackage(id) {
	            parent.right.document.location.href = 'developer/packages/installedPackage.aspx?id=' + id;
            }
            ");
        }

    }
    
}
