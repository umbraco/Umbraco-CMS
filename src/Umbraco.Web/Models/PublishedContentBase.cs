using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
	/// <remarks>
	/// This also ensures that we have an OwnersCollection property so that the IsFirst/IsLast/Index helper methods work 
    /// when referenced inside the result of a collection. http://issues.umbraco.org/issue/U4-1797
	/// </remarks>
    [DebuggerDisplay("Content Id: {Id}, Name: {Name}")]
    public abstract class PublishedContentBase : IPublishedContent, IOwnerCollectionAware<IPublishedContent>
	{
		private string _url;
		private readonly Dictionary<string, object> _resolvePropertyValues = new Dictionary<string, object>();
        private IEnumerable<IPublishedContent> _ownersCollection;

        /// <summary>
        /// Need to get/set the owner collection when an item is returned from the result set of a query
        /// </summary>
        /// <remarks>
        /// Based on this issue here: http://issues.umbraco.org/issue/U4-1797
        /// </remarks>
        IEnumerable<IPublishedContent> IOwnerCollectionAware<IPublishedContent>.OwnersCollection
        {
            get
            {
                //if the owners collection is null, we'll default to it's siblings
                if (_ownersCollection == null)
                {
                    //get the root docs if parent is null
                    _ownersCollection = this.Siblings();
                }
                return _ownersCollection;
            }
            set { _ownersCollection = value; }
        }

	    /// <summary>
		/// Returns the Url for this content item
		/// </summary>
		/// <remarks>
		/// If this item type is media, the Url that is returned is the Url computed by the NiceUrlProvider, otherwise if it is media
		/// the Url returned is the value found in the 'umbracoFile' property.
		/// </remarks>
		public virtual string Url
		{
			get
			{
				if (_url != null)
					return _url;

				switch (ItemType)
				{
					case PublishedItemType.Content:
						if (UmbracoContext.Current == null)
							throw new InvalidOperationException("Cannot resolve a Url for a content item with a null UmbracoContext.Current reference");
						if (UmbracoContext.Current.NiceUrlProvider == null)
							throw new InvalidOperationException("Cannot resolve a Url for a content item with a null UmbracoContext.Current.NiceUrlProvider reference");
						_url= UmbracoContext.Current.NiceUrlProvider.GetNiceUrl(this.Id);
						break;
					case PublishedItemType.Media:
						var prop = GetProperty("umbracoFile");
						if (prop == null)
							throw new NotSupportedException("Cannot retreive a Url for a media item if there is no 'umbracoFile' property defined");
						_url = prop.Value.ToString();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				return _url;
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
			get
			{
				//check this instance's cache, this is better for performance because resolving a value can 
				//have performance impacts since it has to resolve Urls and IPropertyEditorValueConverter's as well.
				if (_resolvePropertyValues.ContainsKey(propertyAlias))
					return _resolvePropertyValues[propertyAlias];
				_resolvePropertyValues.Add(propertyAlias, this.GetPropertyValue(propertyAlias));
				return _resolvePropertyValues[propertyAlias];
			}
		}

		public abstract IPublishedContentProperty GetProperty(string alias);
		public abstract IPublishedContent Parent { get; }
		public abstract IEnumerable<IPublishedContent> Children { get; }

    }
}
