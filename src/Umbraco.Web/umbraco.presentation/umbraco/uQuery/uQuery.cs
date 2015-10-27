using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco
{
	/// <summary>
	/// uQuery - static helper methods
	/// </summary>
	public static partial class uQuery
	{
		/// <summary>
		/// Gets the SqlHelper used by Umbraco
		/// </summary>
		public static ISqlHelper SqlHelper
		{
			get
			{
				return Application.SqlHelper;
			}
		}

		/// <summary>
		/// Constructs the XML source from the cmsContentXml table used for Media and Members.
		/// </summary>
		/// <param name="umbracoObjectType">an UmbracoObjectType value</param>
		/// <returns>XML built from the cmsContentXml table</returns>
		public static XmlDocument GetPublishedXml(UmbracoObjectType umbracoObjectType)
		{
			try
			{
				var hierarchy = new Dictionary<int, List<int>>();
				var nodeIndex = new Dictionary<int, XmlNode>();
				var xmlDoc = new XmlDocument();
				var sql = "SELECT umbracoNode.id, umbracoNode.parentId, umbracoNode.sortOrder, cmsContentXml.xml FROM umbracoNode INNER JOIN cmsContentXml ON cmsContentXml.nodeId = umbracoNode.id AND umbracoNode.nodeObjectType = @nodeObjectType ORDER BY umbracoNode.level, umbracoNode.sortOrder";

				using (var dr = uQuery.SqlHelper.ExecuteReader(sql, uQuery.SqlHelper.CreateParameter("@nodeObjectType", umbracoObjectType.GetGuid())))
				{
					while (dr.Read())
					{
						var currentId = dr.GetInt("id");
						var parentId = dr.GetInt("parentId");
						var xml = dr.GetString("xml");

						// and parse it into a DOM node
						xmlDoc.LoadXml(xml);
						nodeIndex.Add(currentId, xmlDoc.FirstChild);

						// Build the content hierarchy
						List<int> children;
						if (!hierarchy.TryGetValue(parentId, out children))
						{
							// No children for this parent, so add one
							children = new List<int>();
							hierarchy.Add(parentId, children);
						}

						children.Add(currentId);
					}

					// Set top level wrapper element
					switch (umbracoObjectType)
					{
						case UmbracoObjectType.Media:
							xmlDoc.LoadXml("<Media id=\"-1\"/>");
							break;

						case UmbracoObjectType.Member:
							xmlDoc.LoadXml("<Members id=\"-1\"/>");
							break;

						default:
							xmlDoc.LoadXml("<Nodes id=\"-1\"/>");
							break;
					}

					// Start building the content tree recursively from the root (-1) node
					GenerateXmlDocument(hierarchy, nodeIndex, -1, xmlDoc.DocumentElement);

					return xmlDoc;
				}
			}
			catch (Exception ex)
			{
                LogHelper.Error<UmbracoHelper>("uQuery error", ex);
			}

			return null;
		}

		/// <summary>
		/// Checks the Umbraco XML Schema version in use
		/// </summary>
		/// <returns>true if using the old XML schema, else false if using the new XML schema</returns>
		public static bool IsLegacyXmlSchema()
		{
			var isLegacyXmlSchema = false;

			try
			{
				isLegacyXmlSchema = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema;
			}
			catch (MissingMethodException)
			{
				// Method doesn't exist so must be using the legacy schema
				isLegacyXmlSchema = true;
			}

			return isLegacyXmlSchema;
		}

		/// <summary>
		/// build a string array from a csv
		/// </summary>
		/// <param name="csv">string of comma separated values</param>
		/// <returns>An array of node ids as string.</returns>
		public static string[] GetCsvIds(string csv)
		{
			string[] ids = null;

			if (!string.IsNullOrEmpty(csv))
			{
				ids = csv.Split(',').Select(s => s.Trim()).ToArray();
			}

			return ids;
		}

		/// <summary>
		/// Gets Ids from known XML fragments (as saved by the MNTP / XPath CheckBoxList)
		/// </summary>
		/// <param name="xml">The Xml</param>
		/// <returns>An array of node ids as integer.</returns>
		public static int[] GetXmlIds(string xml)
		{
			var ids = new List<int>();

			if (!string.IsNullOrEmpty(xml))
			{
				using (var xmlReader = XmlReader.Create(new StringReader(xml)))
				{
					try
					{
						xmlReader.Read();

						// Check name of first element
						switch (xmlReader.Name)
						{
							case "MultiNodePicker":
							case "XPathCheckBoxList":
							case "CheckBoxTree":

								// Position on first <nodeId>
								xmlReader.ReadStartElement();

								while (!xmlReader.EOF)
								{
									if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "nodeId")
									{
										int id;
										if (int.TryParse(xmlReader.ReadElementContentAsString(), out id))
										{
											ids.Add(id);
										}
									}
									else
									{
										// Step the reader on
										xmlReader.Read();
									}
								}

								break;
						}
					}
					catch
					{
						// Failed to read as Xml
					}
				}
			}

			return ids.ToArray();
		}

		/// <summary>
		/// Gets an Id value from the QueryString
		/// </summary>
		/// <returns>an id as a string or string.empty</returns>
		public static string GetIdFromQueryString()
		{
			var queryStringId = string.Empty;

			if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["id"]))
			{
				queryStringId = HttpContext.Current.Request.QueryString["id"];
			}
			else if (HttpContext.Current.Request.CurrentExecutionFilePathExtension == ".asmx"
					&& HttpContext.Current.Request.UrlReferrer != null
					&& !string.IsNullOrEmpty(HttpContext.Current.Request.UrlReferrer.Query))
			{
				// Special case for MNTP CustomTreeService.asmx
				queryStringId = HttpUtility.ParseQueryString(HttpContext.Current.Request.UrlReferrer.Query)["id"];
			}

			return queryStringId;
		}

		/// <summary>
		/// Converts a string array into an integer array.
		/// </summary>
		/// <param name="items">The string array.</param>
		/// <returns>Returns an integer array.</returns>
		public static int[] ConvertToIntArray(string[] items)
		{
			if (items == null)
				return new int[] { };

			int n;
			return items.Select(s => int.TryParse(s, out n) ? n : 0).ToArray();
		}

		/// <summary>
		/// Generates an XML document.
		/// </summary>
		/// <param name="hierarchy">The hierarchy.</param>
		/// <param name="nodeIndex">Index of the node.</param>
		/// <param name="parentId">The parent id.</param>
		/// <param name="parentNode">The parent node.</param>
		private static void GenerateXmlDocument(IDictionary<int, List<int>> hierarchy, IDictionary<int, XmlNode> nodeIndex, int parentId, XmlNode parentNode)
		{
			List<int> children;

			if (hierarchy.TryGetValue(parentId, out children))
			{
				var childContainer = uQuery.IsLegacyXmlSchema() || string.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME) ? parentNode : parentNode.SelectSingleNode(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME);

				if (!uQuery.IsLegacyXmlSchema() && !string.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
				{
					if (childContainer == null)
					{
						childContainer = xmlHelper.addTextNode(parentNode.OwnerDocument, UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME, string.Empty);
						parentNode.AppendChild(childContainer);
					}
				}

				foreach (int childId in children)
				{
					var childNode = nodeIndex[childId];

					if (uQuery.IsLegacyXmlSchema() || string.IsNullOrEmpty(UmbracoSettings.TEMP_FRIENDLY_XML_CHILD_CONTAINER_NODENAME))
					{
						parentNode.AppendChild(childNode);
					}
					else
					{
						childContainer.AppendChild(childNode);
					}

					// Recursively build the content tree under the current child
					GenerateXmlDocument(hierarchy, nodeIndex, childId, childNode);
				}
			}
		}
	}
}