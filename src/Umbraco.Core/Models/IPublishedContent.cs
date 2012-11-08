using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Models
{
	/// <summary>
	/// Defines a PublishedContent in Umbraco
	/// </summary>
	/// <remarks>
	/// A replacement for INode which needs to occur since INode doesn't contain the document type alias
	/// and INode is poorly formatted with mutable properties (i.e. Lists instead of IEnumerable)
	/// </remarks>
	public interface IPublishedContent
	{
		IPublishedContent Parent { get; }
		int Id { get; }
		int TemplateId { get; }
		int SortOrder { get; }
		string Name { get; }
		string UrlName { get; }
		string DocumentTypeAlias { get; }
		int DocumentTypeId { get; }
		string WriterName { get; }
		string CreatorName { get; }
		int WriterId { get; }
		int CreatorId { get; }
		string Path { get; }
		DateTime CreateDate { get; }
		DateTime UpdateDate { get; }
		Guid Version { get; }
		int Level { get; }
		Collection<IPublishedContentProperty> Properties { get; }
		IEnumerable<IPublishedContent> Children { get; }

		/// <summary>
		/// Returns a property on the object based on an alias
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		/// <remarks>
		/// Although we do have a a property to return Properties of the object, in some cases a custom implementation may not know
		/// about all properties until specifically asked for one by alias. 
		/// 
		/// This method is mostly used in places such as DynamicPublishedContent when trying to resolve a property based on an alias. 
		/// In some cases Pulish Stores, a property value may exist in multiple places and we need to fallback to different cached locations
		/// therefore sometimes the 'Properties' collection may not be sufficient.
		/// </remarks>
		IPublishedContentProperty GetProperty(string alias);
	}
}