using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Templates;

namespace Umbraco.Web.Models
{

	/// <summary>
	/// An abstract base class to use for IPublishedContent which ensures that the Url and Indexed property return values
	/// are consistently returned.
	/// </summary>
	public abstract class PublishedContentBase : IPublishedContent
	{
		/// <summary>
		/// Returns the Url for this content item
		/// </summary>
		public virtual string Url
		{
			get
			{
				if (UmbracoContext.Current == null)
					throw new InvalidOperationException("Cannot resolve a Url for a content item with a null UmbracoContext.Current reference");
				if (UmbracoContext.Current.NiceUrlProvider == null)
					throw new InvalidOperationException("Cannot resolve a Url for a content item with a null UmbracoContext.Current.NiceUrlProvider reference");
				return UmbracoContext.Current.NiceUrlProvider.GetNiceUrl(this.Id);
			}
		}

		public abstract PublishedItemType ItemType { get; }
		public abstract int Id { get; }
		public abstract int TemplateId { get; }
		public abstract int SortOrder { get; }
		public abstract string Name { get; }
		public abstract string UrlName { get; }
		public abstract string DocumentTypeAlias { get; }
		public abstract int DocumentTypeId { get; }
		public abstract string WriterName { get; }
		public abstract string CreatorName { get; }
		public abstract int WriterId { get; }
		public abstract int CreatorId { get; }
		public abstract string Path { get; }
		public abstract DateTime CreateDate { get; }
		public abstract DateTime UpdateDate { get; }
		public abstract Guid Version { get; }
		public abstract int Level { get; }	
		public abstract ICollection<IPublishedContentProperty> Properties { get; }

		/// <summary>
		/// Returns the property value for the property alias specified
		/// </summary>
		/// <param name="propertyAlias"></param>
		/// <returns></returns>
		/// <remarks>
		/// Ensures that the value is executed through the IPropertyEditorValueConverters and that all internal links are are to date
		/// </remarks>
		public virtual object this[string propertyAlias]
		{
			get { return this.GetPropertyValue(propertyAlias); }
		}

		public abstract IPublishedContentProperty GetProperty(string alias);
		public abstract IPublishedContent Parent { get; }
		public abstract IEnumerable<IPublishedContent> Children { get; }
	}
}
