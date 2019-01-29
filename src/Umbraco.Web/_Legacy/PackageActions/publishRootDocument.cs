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
    public class PublishRootDocument : IPackageAction
    {
        #region IPackageAction Members

        /// <summary>
        /// Executes the specified package action.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <example>
        /// <Action runat="install" alias="publishRootDocument" documentName="News"  />
        /// </example>
        /// <returns>True if executed succesfully</returns>
        public bool Execute(string packageName, XElement xmlData)
        {

            string documentName = xmlData.AttributeValue<string>("documentName");

            var rootDocs = Current.Services.ContentService.GetRootContent();

            foreach (var rootDoc in rootDocs)
            {
                if (rootDoc.Name.Trim() == documentName.Trim() && rootDoc.ContentType != null)
                {
                    // TODO: variants?
                    Current.Services.ContentService.SaveAndPublishBranch(rootDoc, true);
                    break;
                }
            }
            return true;
        }

        /// <summary>
        /// This action has no undo.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns></returns>
        public bool Undo(string packageName, XElement xmlData)
        {
            return true;
        }

        /// <summary>
        /// Action alias
        /// </summary>
        /// <returns></returns>
        public string Alias()
        {
            return "publishRootDocument";
        }
        #endregion
        
    }
}
