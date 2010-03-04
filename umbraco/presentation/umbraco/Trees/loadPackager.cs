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
    /// <summary>
    /// Handles loading of the packager application into the developer application tree
    /// </summary>
    public class loadPackager : ITree
    {
        #region TreeI Members

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

        /// <summary>
        /// Renders the Javascript.
        /// </summary>
        /// <param name="Javascript">The javascript.</param>
        public void RenderJS(ref StringBuilder Javascript)
        {
            Javascript.Append(
            @"function openPackageCategory(url) {
			UmbClientMgr.contentFrame('developer/packages/' + url);}"
            );
        }

        /// <summary>
        /// Renders the specified tree item.
        /// </summary>
        /// <param name="Tree">The tree.</param>
        public void Render(ref XmlDocument Tree)
        {
            XmlElement root = Tree.DocumentElement;
            
            string[,] items = { { "BrowseRepository.aspx", "Install from repository" }, { "CreatePackage.aspx", "Createdjjj Packages" }, { "installedPackages.aspx", "Installedjj packages" }, { "boost.aspx", "Boost" }, { "installer.aspx", "Install local package" } };


            for (int i = 0; i <= items.GetUpperBound(0); i++)
            {

                XmlElement treeElement = Tree.CreateElement("tree");
                treeElement.SetAttribute("nodeID", "-1");
                treeElement.SetAttribute("text", items[i, 1]);
                treeElement.SetAttribute("icon", "folder.gif");
                treeElement.SetAttribute("openIcon", "folder_o.gif");

                //Make sure the different sections load the correct childnodes.
                switch (items[i, 0])
                {
                    case "installedPackages.aspx":
                        if (cms.businesslogic.packager.InstalledPackage.GetAllInstalledPackages().Count > 0)
                        {
                            treeElement.SetAttribute("src", "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=installed" + "&rnd=" + Guid.NewGuid());
                            treeElement.SetAttribute("nodeType", "installedPackages");
                            treeElement.SetAttribute("menu", "L");
                            treeElement.SetAttribute("text", ui.Text("treeHeaders", "installedPackages"));
                            treeElement.SetAttribute("hasChildren", "true");
                        }
                        else
                        {
                            treeElement.SetAttribute("text", "");
                        }
                        break;


                    case "BrowseRepository.aspx":

                        //Gets all the repositories registered in umbracoSettings.config
                        List<cms.businesslogic.packager.repositories.Repository> repos = cms.businesslogic.packager.repositories.Repository.getAll();


                        //if more then one repo, then list them as child nodes under the "Install from repository" node.
                        // the repositories will then be fetched from the loadPackages class.
                        if (repos.Count > 1)
                        {
                            treeElement.SetAttribute("src", "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=repositories" + "&rnd=" + Guid.NewGuid());
                            treeElement.SetAttribute("nodeType", "packagesRepositories");
                            treeElement.SetAttribute("menu", "L");
                            treeElement.SetAttribute("text", ui.Text("treeHeaders", "repositories"));
                            treeElement.SetAttribute("hasChildren", "true");
                        }

                        //if only one repo, then just list it directly and name it as the repository.
                        //the packages will be loaded from the loadPackages class with a repoAlias querystring
                        else if (repos.Count == 1)
                        {
                            treeElement.SetAttribute("text", repos[0].Name);
                            treeElement.SetAttribute("src", "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=repository&repoGuid=" + repos[0].Guid + "&rnd=" + Guid.NewGuid());
                            treeElement.SetAttribute("nodeType", "packagesRepository");
                            treeElement.SetAttribute("menu", "L");
                            treeElement.SetAttribute("action", "javascript:openPackageCategory('BrowseRepository.aspx?repoGuid=" + repos[0].Guid + "');");
                            treeElement.SetAttribute("icon", "repository.gif");
                            treeElement.SetAttribute("openIcon", "repository.gif");
                            treeElement.SetAttribute("hasChildren", "true");
                        }

                       //if none registered, then remove the repo node.
                        else if (repos.Count == 0)
                        {
                            treeElement.SetAttribute("text", "");
                        }

                        break;


                    case "CreatePackage.aspx":
                        treeElement.SetAttribute("src", "tree.aspx?app=" + this._app + "&id=" + this._id + "&treeType=packagerPackages&packageType=created" + "&rnd=" + Guid.NewGuid());
                        treeElement.SetAttribute("nodeType", "createdPackages");
                        treeElement.SetAttribute("menu", "C,L");
                        treeElement.SetAttribute("text", ui.Text("treeHeaders", "createdPackages"));
                        treeElement.SetAttribute("hasChildren", "true");
                        break;

                    case "installer.aspx":
                        treeElement.SetAttribute("src", "");
                        treeElement.SetAttribute("nodeType", "uploadPackage");
                        //treeElement.SetAttribute("menu", "");
                        treeElement.SetAttribute("icon", "uploadpackage.gif");
                        treeElement.SetAttribute("openIcon", "uploadpackage.gif");
                        treeElement.SetAttribute("action", "javascript:openPackageCategory('" + items[i, 0] + "');");
                        treeElement.SetAttribute("text", ui.Text("treeHeaders", "localPackage"));
                        break;

                    case "boost.aspx":
                        treeElement.SetAttribute("src", "");
                        treeElement.SetAttribute("nodeType", "packagesBoost");
                        //treeElement.SetAttribute("menu", "L");
                        treeElement.SetAttribute("action", "javascript:openPackageCategory('" + items[i, 0] + "');");
                        treeElement.SetAttribute("icon", "nitros.gif");
                        treeElement.SetAttribute("openIcon", "nitros.gif");
                        treeElement.SetAttribute("text", ui.Text("treeHeaders", "runwayModules"));

                        if (!cms.businesslogic.packager.InstalledPackage.isPackageInstalled("ae41aad0-1c30-11dd-bd0b-0800200c9a66"))
                            treeElement.SetAttribute("text", ui.Text("treeHeaders", "runway"));
                        break;

                    default:
                        break;
                }

                if (treeElement.GetAttribute("text") != "")
                    root.AppendChild(treeElement);

            }

        }
    }

        #endregion
    
}
