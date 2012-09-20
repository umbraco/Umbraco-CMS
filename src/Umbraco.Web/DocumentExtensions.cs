using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic;
using Umbraco.Core;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for IDocument
	/// </summary>
	/// <remarks>
	/// These methods exist in the web project as we need access to web based classes like NiceUrl provider
	/// which is why they cannot exist in the Core project.
	/// </remarks>
	public static class DocumentExtensions
	{	

		/// <summary>
		/// Returns a DataTable object for the IDocument
		/// </summary>
		/// <param name="d"></param>
		/// <param name="nodeTypeAliasFilter"></param>
		/// <returns></returns>
		public static DataTable ChildrenAsTable(this IDocument d, string nodeTypeAliasFilter = "")
		{
			return GenerateDataTable(d, nodeTypeAliasFilter);
		}

		/// <summary>
		/// Generates the DataTable for the IDocument
		/// </summary>
		/// <param name="node"></param>
		/// <param name="nodeTypeAliasFilter"> </param>
		/// <returns></returns>
		private static DataTable GenerateDataTable(IDocument node, string nodeTypeAliasFilter = "")
		{
			var firstNode = nodeTypeAliasFilter.IsNullOrWhiteSpace()
			                	? node.Children.Any()
			                	  	? node.Children.ElementAt(0)
			                	  	: null
			                	: node.Children.FirstOrDefault(x => x.DocumentTypeAlias == nodeTypeAliasFilter);
			if (firstNode == null)
				return new DataTable(); //no children found 

			var urlProvider = UmbracoContext.Current.RoutingContext.NiceUrlProvider;

			//use new utility class to create table so that we don't have to maintain code in many places, just one
			var dt = Umbraco.Core.DataTableExtensions.GenerateDataTable(
				//pass in the alias of the first child node since this is the node type we're rendering headers for
				firstNode.DocumentTypeAlias,
				//pass in the callback to extract the Dictionary<string, string> of all defined aliases to their names
				alias => GetPropertyAliasesAndNames(alias),
				//pass in a callback to populate the datatable, yup its a bit ugly but it's already legacy and we just want to maintain code in one place.
				() =>
					{
						//create all row data
						var tableData = Umbraco.Core.DataTableExtensions.CreateTableData();
						//loop through each child and create row data for it
						foreach (var n in node.Children)
						{
							if (!nodeTypeAliasFilter.IsNullOrWhiteSpace())
							{
								if (n.DocumentTypeAlias != nodeTypeAliasFilter)
									continue; //skip this one, it doesn't match the filter
							}

							var standardVals = new Dictionary<string, object>()
								{
									{"Id", n.Id},
									{"NodeName", n.Name},
									{"NodeTypeAlias", n.DocumentTypeAlias},
									{"CreateDate", n.CreateDate},
									{"UpdateDate", n.UpdateDate},
									{"CreatorName", n.CreatorName},
									{"WriterName", n.WriterName},
									{"Url", urlProvider.GetNiceUrl(n.Id)}
								};
							var userVals = new Dictionary<string, object>();
							foreach (var p in from IDocumentProperty p in n.Properties where p.Value != null select p)
							{
								userVals[p.Alias] = p.Value;
							}
							//add the row data
							Umbraco.Core.DataTableExtensions.AddRowData(tableData, standardVals, userVals);
						}
						return tableData;
					}
				);
			return dt;
		}

		private static Func<string, Dictionary<string, string>> _getPropertyAliasesAndNames;
		
		/// <summary>
		/// This is used only for unit tests to set the delegate to look up aliases/names dictionary of a content type
		/// </summary>
		internal static Func<string, Dictionary<string, string>> GetPropertyAliasesAndNames
		{
			get
			{
				return _getPropertyAliasesAndNames ?? (_getPropertyAliasesAndNames = alias =>
					{
						var userFields = ContentType.GetAliasesAndNames(alias);
						//ensure the standard fields are there
						var allFields = new Dictionary<string, string>()
							{
								{"Id", "Id"},
								{"NodeName", "NodeName"},
								{"NodeTypeAlias", "NodeTypeAlias"},
								{"CreateDate", "CreateDate"},
								{"UpdateDate", "UpdateDate"},
								{"CreatorName", "CreatorName"},
								{"WriterName", "WriterName"},
								{"Url", "Url"}
							};
						foreach (var f in userFields.Where(f => !allFields.ContainsKey(f.Key)))
						{
							allFields.Add(f.Key, f.Value);
						}
						return allFields;
					});
			}
			set { _getPropertyAliasesAndNames = value; }
		}
	}
}