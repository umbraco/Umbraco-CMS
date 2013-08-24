using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
    /// <summary>
    /// This class implements the IPackageAction Interface, used to execute code when packages are installed.
    /// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
    /// </summary>
    public class RunDatabaseMigration : umbraco.interfaces.IPackageAction
    {
        #region IPackageAction Members
        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>true if the action executed successfully, false otherwise.</returns>
        /// <example><code>
        /// <Action runat="install" [undo="false"] alias="addApplicationTree" silent="[true/false]"  initialize="[true/false]" sortOrder="1" 
        /// applicationAlias="appAlias" treeAlias="myTree" treeTitle="My Tree" iconOpened="folder_o.gif" iconClosed="folder.gif"
        /// assemblyName="umbraco" treeHandlerType="treeClass" action="alert('you clicked my tree')"/>
        /// </code></example>
        public bool Execute(string packageName, XmlNode xmlData)
        {
            Version targetVersion, configuredVersion;
            string versionStr = xmlData.Attributes["targetVersion"].Value;
            if (!Version.TryParse(versionStr, out targetVersion))
            {
                return false;
            }
            versionStr = string.Empty;
            versionStr = xmlData.Attributes["configuredVersion"].Value;
            if (!Version.TryParse(versionStr, out configuredVersion))
            {
                return false;
            }
            var runner = new MigrationRunner(configuredVersion, targetVersion, packageName);
            return runner.Execute(ApplicationContext.Current.DatabaseContext.Database);
        }
        /// <summary>
        /// Action alias.
        /// </summary>
        /// <returns></returns>
        public string Alias()
        {
            return "RunDatabaseMigration";
        }
        /// <summary>
        /// Undoes the action
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>true if the undo action succeeds, false otherwise.</returns>
        public bool Undo(string packageName, XmlNode xmlData)
        {
            Version targetVersion, configuredVersion;
            string versionStr = xmlData.Attributes["targetVersion"].Value;
            if (!Version.TryParse(versionStr, out targetVersion))
            {
                return false;
            }
            versionStr = string.Empty;
            versionStr = xmlData.Attributes["configuredVersion"].Value;
            if (!Version.TryParse(versionStr, out configuredVersion))
            {
                return false;
            }
            var runner = new MigrationRunner(configuredVersion, targetVersion, packageName);
            return runner.Execute(ApplicationContext.Current.DatabaseContext.Database, false);
        }
        #endregion
        public XmlNode SampleXml()
        {
            string sample = "<Action runat=\"install\" undo=\"true/false\" alias=\"runDatabaseMigration\" configuredVersion=\"6.1.0.0\"  targetVersion=\"6.1.3.0\"/>";
            return helper.parseStringToXmlNode(sample);
        }
    }
}