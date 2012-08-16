using System;
using System.Collections.Generic;
using Umbraco.Core.Dynamics;
using umbraco.cms.businesslogic;

namespace Umbraco.Web
{

	/// <summary>
	/// This exists only because we want Dynamics in the Core project but DynamicNode has references to ContentType to run some queries
	/// and currently the business logic part of Umbraco is still in the legacy project and we don't want to move that to the core so in the
	/// meantime until the new APIs are made, we need to have this data source in place with a resolver which is set in the web project.
	/// </summary>
	internal class DefaultDynamicNodeDataSource : IDynamicNodeDataSource
	{
		public IEnumerable<string> GetAncestorOrSelfNodeTypeAlias(DynamicBackingItem node)
		{
			var list = new List<string>();
			if (node != null)
			{
				if (node.Type == DynamicBackingItemType.Content)
				{
					//find the doctype node, so we can walk it's parent's tree- not the working.parent content tree
					CMSNode working = ContentType.GetByAlias(node.NodeTypeAlias);
					while (working != null)
					{
						if ((working as ContentType) != null)
						{
							list.Add((working as ContentType).Alias);
						}
						try
						{
							working = working.Parent;
						}
						catch (ArgumentException)
						{
							break;
						}
					}
				}
				else
				{
					return null;
				}
			}
			return list;
		}

		public Guid GetDataType(string contentTypeAlias, string propertyTypeAlias)
		{
			return ContentType.GetDataType(contentTypeAlias, propertyTypeAlias);
		}
	}
}