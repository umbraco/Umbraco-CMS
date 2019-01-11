using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Xml;
using Umbraco.Core._Legacy.PackageActions;

namespace Umbraco.Web._Legacy.PackageActions
{
    /// <summary>
    ///
    /// </summary>
    public class addDashboardSection : IPackageAction
    {
        #region IPackageAction Members

        /// <summary>
        /// Installs a dashboard section. This action reuses the action XML, so it has to be valid dashboard markup.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="xmlData">The XML data.</param>
        /// <returns>true if successfull</returns>
        /// <example>
        /// <code>
        /// <Action runat="install" [undo="false"] alias="addDashboardSection" dashboardAlias="MyDashboardSection">
        ///     <section>
        ///         <areas>
        ///         <area>default</area>
        ///         <area>content</area>
        ///         </areas>
        ///            <tab caption="Last Edits">
        ///             <control>/usercontrols/dashboard/latestEdits.ascx</control>
        ///             <control>/usercontrols/umbracoBlog/dashboardBlogPostCreate.ascx</control>
        ///         </tab>
        ///         <tab caption="Create blog post">
        ///             <control>/usercontrols/umbracoBlog/dashboardBlogPostCreate.ascx</control>
        ///         </tab>
        ///     </section>
        /// </Action>
        /// </code>
        /// </example>
        public bool Execute(string packageName, XElement xmlData)
        {
            //this will need a complete section node to work...

            if (xmlData.HasElements)
            {
                string sectionAlias = xmlData.AttributeValue<string>("dashboardAlias");
                string dbConfig = SystemFiles.DashboardConfig;

                var section = xmlData.Element("section");
                var dashboardFile = XDocument.Load(IOHelper.MapPath(dbConfig));

                //don't continue if it already exists
                var found = dashboardFile.XPathSelectElements("//section[@alias='" + sectionAlias + "']");
                if (!found.Any())
                {
                    dashboardFile.Root.Add(section);
                    dashboardFile.Save(IOHelper.MapPath(dbConfig));
                }

                return true;
            }

            return false;
        }


        public string Alias()
        {
            return "addDashboardSection";
        }

        public bool Undo(string packageName, XElement xmlData)
        {

            string sectionAlias = xmlData.AttributeValue<string>("dashboardAlias");
            string dbConfig = SystemFiles.DashboardConfig;
            var dashboardFile = XDocument.Load(IOHelper.MapPath(dbConfig));

            var section = dashboardFile.XPathSelectElement("//section [@alias = '" + sectionAlias + "']");

            if (section != null)
            {
                section.Remove();
                dashboardFile.Save(IOHelper.MapPath(dbConfig));
            }

            return true;
        }

        #endregion
        
    }
}
