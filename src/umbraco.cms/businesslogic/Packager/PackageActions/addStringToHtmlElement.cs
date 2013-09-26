using System;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace umbraco.cms.businesslogic.packager.standardPackageActions
{
	/// <summary>
	/// This class implements the IPackageAction Interface, used to execute code when packages are installed.
	/// All IPackageActions only takes a PackageName and a XmlNode as input, and executes based on the data in the xmlnode.
	/// addStringToHtmlElement adds a string to specific HTML element in a specific template, and can either append or prepend it.
	/// It uses the action xml node to do this, exemple action xml node:
	/// <Action runat="install" alias="addStringToHtmlElement" templateAlias="news" htmlElementId="newsSection" position="end"><![CDATA[hello world!]]></action>
	/// The above will add the string "hello world!" to the first html element with the id "newsSection" in the template "news"
	/// </summary>
	public class addStringToHtmlElement : umbraco.interfaces.IPackageAction
	{
		#region IPackageAction Members

		/// <summary>
		/// Executes the specified package action.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <example><code><code> 
		///     <Action runat="install" alias="addStringToHtmlElement" templateAlias="news" htmlElementId="newsSection" position="[beginning/end"><![CDATA[hello world!]]></action>
		/// </code></code></example>
		/// <returns>True if executed successfully</returns>
		public bool Execute(string packageName, XmlNode xmlData)
		{


			string templateAlias = xmlData.Attributes["templateAlias"].Value;
			string htmlElementId = xmlData.Attributes["htmlElementId"].Value;
			string position = xmlData.Attributes["position"].Value;
			string value = xmlHelper.GetNodeValue(xmlData);
			template.Template tmp = template.Template.GetByAlias(templateAlias);

            if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
				value = tmp.EnsureMasterPageSyntax(value);

			_addStringToHtmlElement(tmp, value, templateAlias, htmlElementId, position);

			return true;
		}


		/// <summary>
		/// Undoes the addStringToHtml Execute() method, by removing the same string from the same template.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <returns></returns>
		public bool Undo(string packageName, XmlNode xmlData)
		{
			string templateAlias = xmlData.Attributes["templateAlias"].Value;
			string htmlElementId = xmlData.Attributes["htmlElementId"].Value;
			string value = xmlHelper.GetNodeValue(xmlData);
			template.Template tmp = template.Template.GetByAlias(templateAlias);

			if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
				value = tmp.EnsureMasterPageSyntax(value);

			_removeStringFromHtmlElement(tmp, value, templateAlias, htmlElementId);
			return true;
		}

		/// <summary>
		/// Action alias.
		/// </summary>
		/// <returns></returns>
		public string Alias()
		{
			return "addStringToHtmlElement";
		}

		private void _addStringToHtmlElement(template.Template tmp, string value, string templateAlias, string htmlElementId, string position)
		{
			bool hasAspNetContentBeginning = false;
			string design = "";
			string directive = "";

			if (tmp != null)
			{
				try
				{
					XmlDocument templateXml = new XmlDocument();
					templateXml.PreserveWhitespace = true;

					//Make sure that directive is remove before hacked non html4 compatiple replacement action... 
					design = tmp.Design;


					splitDesignAndDirective(ref design, ref directive);

					//making sure that the template xml has a root node...
					if (tmp.MasterTemplate > 0)
						templateXml.LoadXml(helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, "<root>" + design + "</root>", true));
					else
						templateXml.LoadXml(helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, design, true));

					XmlNode xmlElement = templateXml.SelectSingleNode("//* [@id = '" + htmlElementId + "']");

					if (xmlElement != null)
					{

						if (position == "beginning")
						{
							xmlElement.InnerXml = "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true) + "\n" + xmlElement.InnerXml;
						}
						else
						{
							xmlElement.InnerXml = xmlElement.InnerXml + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true) + "\n";
						}
					}

					tmp.Design = directive + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, templateXml.OuterXml, false);
					tmp.Save();
				}
				catch (Exception ex)
				{
					LogHelper.Error<addStringToHtmlElement>("An error occurred", ex);
				}
			}
			else
			{
				LogHelper.Debug<addStringToHtmlElement>("template not found");
			}
		}

		private void _removeStringFromHtmlElement(template.Template tmp, string value, string templateAlias, string htmlElementId)
		{
			bool hasAspNetContentBeginning = false;
			string design = "";
			string directive = "";


			if (tmp != null)
			{
				try
				{
					XmlDocument templateXml = new XmlDocument();
					templateXml.PreserveWhitespace = true;

					//Make sure that directive is remove before hacked non html4 compatiple replacement action... 
					design = tmp.Design;
					splitDesignAndDirective(ref design, ref directive);

					//making sure that the template xml has a root node...
					if (tmp.MasterTemplate > 0)
						templateXml.LoadXml(helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, "<root>" + design + "</root>", true));
					else
						templateXml.LoadXml(helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, design, true));

					XmlNode xmlElement = templateXml.SelectSingleNode("//* [@id = '" + htmlElementId + "']");



					if (xmlElement != null)
					{
						string repValue = helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, value, true);
						xmlElement.InnerXml = xmlElement.InnerXml.Replace(repValue, "");
					}

					tmp.Design = directive + "\n" + helper.parseToValidXml(tmp, ref hasAspNetContentBeginning, templateXml.OuterXml, false);
					tmp.Save();
				}
				catch (Exception ex)
				{
					LogHelper.Error<addStringToHtmlElement>("An error occurred", ex);
				}
			}
			else
			{
				LogHelper.Debug<addStringToHtmlElement>("template not found");
			}
		}



		private void splitDesignAndDirective(ref string design, ref string directive)
		{
			if (design.StartsWith("<%@"))
			{
				directive = design.Substring(0, design.IndexOf("%>") + 2).Trim(Environment.NewLine.ToCharArray());
				design = design.Substring(design.IndexOf("%>") + 3).Trim(Environment.NewLine.ToCharArray());
			}
		}

		#endregion

		public XmlNode SampleXml()
		{
			throw new NotImplementedException();
		}

	}
}