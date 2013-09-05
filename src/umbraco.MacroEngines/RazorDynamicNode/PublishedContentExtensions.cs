using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web;
using umbraco.NodeFactory;
using umbraco.interfaces;
using Property = umbraco.NodeFactory.Property;

namespace umbraco.MacroEngines.Library
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedContent</c>.
    /// </summary>
    /// <remarks>These are dedicated to converting DynamicPublishedContent to INode.</remarks>
	internal static class PublishedContentExtensions
	{		
		internal static IProperty ConvertToNodeProperty(this IPublishedProperty prop)
		{
			return new PropertyResult(prop.Alias, prop.Value.ToString());
		}

		internal static INode ConvertToNode(this IPublishedContent doc)
		{
			var node = new ConvertedNode(doc);	
			return node;
		}

		/// <summary>
		/// Internal custom INode class used for conversions from DynamicPublishedContent.
		/// </summary>
		private class ConvertedNode : INode
		{
			private readonly IPublishedContent _doc;

			public ConvertedNode(IPublishedContent doc)
			{
				_doc = doc;
				template = doc.TemplateId;
				Id = doc.Id;
				Path = doc.Path;
				CreatorName = doc.CreatorName;
				SortOrder = doc.SortOrder;
				UpdateDate = doc.UpdateDate;
				Name = doc.Name;
				NodeTypeAlias = doc.DocumentTypeAlias;
				CreateDate = doc.CreateDate;
				CreatorID = doc.CreatorId;
				Level = doc.Level;
				UrlName = doc.UrlName;
				Version = doc.Version;
				WriterID = doc.WriterId;
				WriterName = doc.WriterName;
			}

			public INode Parent
			{
				get { return _doc.Parent.ConvertToNode(); }
			}
			public int Id { get; private set; }
			public int template { get; private set; }
			public int SortOrder { get; private set; }
			public string Name { get; private set; }
			public string UrlName { get; private set; }
			public string NodeTypeAlias { get; private set; }
			public string WriterName { get; private set; }
			public string CreatorName { get; private set; }
			public int WriterID { get; private set; }
			public int CreatorID { get; private set; }
			public string Path { get; private set; }
			public DateTime CreateDate { get; private set; }
			public DateTime UpdateDate { get; private set; }
			public Guid Version { get; private set; }

			public string NiceUrl
			{
				get { return library.NiceUrl(Id); }
			}

			public string Url
			{
				get { return library.NiceUrl(Id); }
			}

			public int Level { get; private set; }
			public List<IProperty> PropertiesAsList
			{
				get { return _doc.Properties.Select(ConvertToNodeProperty).ToList(); }
			}
			public List<INode> ChildrenAsList
			{
				get { return _doc.Children.Select(x => x.ConvertToNode()).ToList(); }
			}
			public IProperty GetProperty(string Alias)
			{
				return PropertiesAsList.Cast<Property>().FirstOrDefault(p => p.Alias == Alias);
			}

			public IProperty GetProperty(string Alias, out bool propertyExists)
			{
				foreach (var p in from Property p in PropertiesAsList where p.Alias == Alias select p)
				{
					propertyExists = true;
					return p;
				}
				propertyExists = false;
				return null;
			}

			public DataTable ChildrenAsTable()
			{
				return _doc.ChildrenAsTable();
			}

			public DataTable ChildrenAsTable(string nodeTypeAliasFilter)
			{
				return _doc.ChildrenAsTable(nodeTypeAliasFilter);
			}
		}
	}
}