using System.Xml;
using Umbraco.Core;
using Umbraco.Core._Legacy.PackageActions;
using Umbraco.Web.Composing;

namespace Umbraco.Web._Legacy.PackageActions
{
    /*Build in standard actions */

    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class addApplicationTree : IPackageAction
    {

        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        /// <example><code>
        /// <Action runat="install" [undo="false"] alias="addApplicationTree" silent="[true/false]"  initialize="[true/false]" sortOrder="1"
        /// applicationAlias="appAlias" treeAlias="myTree" treeTitle="My Tree" iconOpened="folder_o.gif" iconClosed="folder.gif"
        /// assemblyName="umbraco" treeHandlerType="treeClass" action="alert('you clicked my tree')"/>
        /// </code></example>
        public bool Execute(string packageName, XmlNode xmlData)
        {
            bool initialize = bool.Parse(xmlData.Attributes["initialize"].Value);
            byte sortOrder = byte.Parse(xmlData.Attributes["sortOrder"].Value);

            string applicationAlias = xmlData.Attributes["applicationAlias"].Value;
            string treeAlias = xmlData.Attributes["treeAlias"].Value;
            string treeTitle = xmlData.Attributes["treeTitle"].Value;
            string iconOpened = xmlData.Attributes["iconOpened"].Value;
            string iconClosed = xmlData.Attributes["iconClosed"].Value;
            string type = xmlData.Attributes["treeHandlerType"].Value;

            Current.Services.ApplicationTreeService.MakeNew(initialize, sortOrder, applicationAlias, treeAlias, treeTitle, iconClosed, iconOpened, type);

            return true;
        }

        /// <summary>
        /// Undoes the action
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XmlNode xmlData)
        {
            string treeAlias = xmlData.Attributes["treeAlias"].Value;
            var found = Current.Services.ApplicationTreeService.GetByAlias(treeAlias);
            if (found != null)
            {
                Current.Services.ApplicationTreeService.DeleteTree(found);
            }
            return true;
        }

        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias()
        {
            return "addApplicationTree";
        }

        #endregion


        public XmlNode SampleXml()
        {

            string sample = "<Action runat=\"install\" undo=\"true/false\" alias=\"addApplicationTree\" silent=\"true/false\"  initialize=\"true/false\" sortOrder=\"1\" applicationAlias=\"appAlias\" treeAlias=\"myTree\" treeTitle=\"My Tree\" iconOpened=\"folder_o.gif\" iconClosed=\"folder.gif\" assemblyName=\"umbraco\" treeHandlerType=\"treeClass\" action=\"alert('you clicked my tree')\"/>";
            return PackageHelper.ParseStringToXmlNode(sample);
        }
    }
}
