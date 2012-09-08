using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Umbraco.Core.Models
{
	/// <summary>
	/// Defines a document in Umbraco
	/// </summary>
	/// <remarks>
	/// A replacement for INode which needs to occur since INode doesn't contain the document type alias
	/// and INode is poorly formatted with mutable properties (i.e. Lists instead of IEnumerable)
	/// </remarks>
	public interface IDocument
	{
		IDocument Parent { get; }
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
		Collection<IDocumentProperty> Properties { get; }
		IEnumerable<IDocument> Children { get; }	
	}
}