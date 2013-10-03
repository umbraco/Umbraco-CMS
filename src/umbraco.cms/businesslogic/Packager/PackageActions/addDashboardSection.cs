using System;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	/// <summary>
	/// 
	/// </summary>
	public class addDashboardSection : umbraco.interfaces.IPackageAction
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
		///	        <tab caption="Last Edits">
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
		public bool Execute(string packageName, XmlNode xmlData)
		{
			//this will need a complete section node to work... 

			if (xmlData.HasChildNodes)
			{
				string sectionAlias = xmlData.Attributes["dashboardAlias"].Value;
				string dbConfig = SystemFiles.DashboardConfig;

				XmlNode section = xmlData.SelectSingleNode("./section");
				XmlDocument dashboardFile = XmlHelper.OpenAsXmlDocument(dbConfig);

				XmlNode importedSection = dashboardFile.ImportNode(section, true);

                XmlAttribute alias = XmlHelper.AddAttribute(dashboardFile, "alias", sectionAlias);
				importedSection.Attributes.Append(alias);

				dashboardFile.DocumentElement.AppendChild(importedSection);

				dashboardFile.Save(IOHelper.MapPath(dbConfig));

				return true;
			}

			return false;
		}


		public string Alias()
		{
			return "addDashboardSection";
		}

		public bool Undo(string packageName, XmlNode xmlData)
		{

			string sectionAlias = xmlData.Attributes["dashboardAlias"].Value;
			string dbConfig = SystemFiles.DashboardConfig;
            XmlDocument dashboardFile = XmlHelper.OpenAsXmlDocument(dbConfig);

			XmlNode section = dashboardFile.SelectSingleNode("//section [@alias = '" + sectionAlias + "']");

			if (section != null)
			{

				dashboardFile.SelectSingleNode("/dashBoard").RemoveChild(section);
				dashboardFile.Save(IOHelper.MapPath(dbConfig));
			}

			return true;
		}

		#endregion

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}