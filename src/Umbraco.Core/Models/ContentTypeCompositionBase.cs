using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for composition specific ContentType properties and methods
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class ContentTypeCompositionBase : ContentTypeBase, IContentTypeComposition
    {
        private readonly List<IContentTypeComposition> _contentTypeComposition = new List<IContentTypeComposition>();

        protected ContentTypeCompositionBase(int parentId) : base(parentId)
        {
        }

		protected ContentTypeCompositionBase(IContentTypeBase parent) : base(parent)
		{
		}

        private static readonly PropertyInfo ContentTypeCompositionSelector =
            ExpressionHelper.GetPropertyInfo<ContentTypeCompositionBase, IEnumerable<IContentTypeComposition>>(
                x => x.ContentTypeComposition);

        /// <summary>
        /// List of ContentTypes that make up a composition of PropertyGroups and PropertyTypes for the current ContentType
        /// </summary>
        [DataMember]
        public IEnumerable<IContentTypeComposition> ContentTypeComposition
        {
            get { return _contentTypeComposition; }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyGroup"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> CompositionPropertyGroups
        {
            get
            {
                var groups = PropertyGroups.Union(ContentTypeComposition.SelectMany(x => x.CompositionPropertyGroups));
                return groups;
            }
        }

        /// <summary>
        /// Returns a list of <see cref="PropertyType"/> objects from the composition
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> CompositionPropertyTypes
        {
            get
            {
                var propertyTypes =
                    PropertyTypes.Union(ContentTypeComposition.SelectMany(x => x.CompositionPropertyTypes));
                return propertyTypes;
            }
        }

        /// <summary>
        /// Adds a new ContentType to the list of composite ContentTypes
        /// </summary>
        /// <param name="contentType"><see cref="ContentType"/> to add</param>
        /// <returns>True if ContentType was added, otherwise returns False</returns>
        public bool AddContentType(IContentTypeComposition contentType)
        {
            if (contentType.ContentTypeComposition.Any(x => x.CompositionAliases().Any(ContentTypeCompositionExists)))
                return false;

            if (!ContentTypeCompositionExists(contentType.Alias))
            {
                _contentTypeComposition.Add(contentType);
                OnPropertyChanged(ContentTypeCompositionSelector);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a ContentType with the supplied alias from the the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType was removed, otherwise returns False</returns>
        public bool RemoveContentType(string alias)
        {
            if (!ContentTypeCompositionExists(alias))
            {
                var contentTypeComposition = ContentTypeComposition.First(x => x.Alias == alias);
                return _contentTypeComposition.Remove(contentTypeComposition);
            }
            return false;
        }

        /// <summary>
        /// Checks if a ContentType with the supplied alias exists in the list of composite ContentTypes
        /// </summary>
        /// <param name="alias">Alias of a <see cref="ContentType"/></param>
        /// <returns>True if ContentType with alias exists, otherwise returns False</returns>
        public bool ContentTypeCompositionExists(string alias)
        {
            if (ContentTypeComposition.Any(x => x.Alias.Equals(alias)))
                return true;

            if (ContentTypeComposition.Any(x => x.ContentTypeCompositionExists(alias)))
                return true;

            return false;
        }

        /// <summary>
        /// Gets a list of ContentType aliases from the current composition 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Does not contain the alias of the Current ContentType</remarks>
        public IEnumerable<string> CompositionAliases()
        {
            return ContentTypeComposition
                .Select(x => x.Alias)
                .Union(ContentTypeComposition.SelectMany(x => x.CompositionAliases()));
        }
    }
}