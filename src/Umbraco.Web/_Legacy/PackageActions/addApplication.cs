using System;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core._Legacy.PackageActions;
using Umbraco.Web.Composing;

namespace Umbraco.Web._Legacy.PackageActions
{
    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class AddApplication : IPackageAction
    {

        #region IPackageAction Members

        /// <summary>
        /// Installs a new application in umbraco.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example><code>
        /// <Action runat="install" [undo="false"] alias="addApplication" appName="Application Name"  appAlias="myApplication" appIcon="application.gif"/>
        /// </code></example>
        /// <returns>true if successful</returns>
        public bool Execute(string packageName, XElement xmlData)
        {
            string name = xmlData.AttributeValue<string>("appName");
            string alias = xmlData.AttributeValue<string>("appAlias");
            string icon = xmlData.AttributeValue<string>("appIcon");

            Current.Services.SectionService.MakeNew(name, alias, icon);

            return true;
        }

        public bool Undo(string packageName, XElement xmlData)
        {
            string alias = xmlData.AttributeValue<string>("appAlias");
            var section = Current.Services.SectionService.GetByAlias(alias);
            if (section != null)
            {
                Current.Services.SectionService.DeleteSection(section);
            }
            return true;
        }
        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias()
        {
            return "addApplication";
        }

        #endregion
        
    }
}
