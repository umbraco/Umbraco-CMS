using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.UI;
using Umbraco.Core.Cache;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedContent"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedContentType"/> class are immutable, ie
    /// if the content type changes, then a new class needs to be created.</remarks>
    public class PublishedContentType
    {
        private readonly PublishedPropertyType[] _propertyTypes;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        // internal so it can be used by PublishedNoCache which does _not_ want to cache anything and so will never
        // use the static cache getter PublishedContentType.GetPublishedContentType(alias) below - anything else
        // should use it.
        internal PublishedContentType(IContentTypeComposition contentType)
        {
            Id = contentType.Id;
            Alias = contentType.Alias;
            _propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => new PublishedPropertyType(this, x))
                .ToArray();
            InitializeIndexes();
        }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            Id = id;
            Alias = alias;
            _propertyTypes = propertyTypes.ToArray();
            foreach (var propertyType in _propertyTypes)
                propertyType.ContentType = this;
            InitializeIndexes();
        }

        // create detached content type - ie does not match anything in the DB
        internal PublishedContentType(string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : this (0, alias, propertyTypes)
        { }

        private void InitializeIndexes()
        {
            for (var i = 0; i < _propertyTypes.Length; i++)
            {
                var propertyType = _propertyTypes[i];
                _indexes[propertyType.PropertyTypeAlias] = i;
                _indexes[propertyType.PropertyTypeAlias.ToLowerInvariant()] = i;
            }
        }

        #region Content type

        public int Id { get; private set; }
        public string Alias { get; private set; }

        #endregion

        #region Properties

        public IEnumerable<PublishedPropertyType> PropertyTypes
        {
            get { return _propertyTypes; }
        }

        // alias is case-insensitive
        // this is the ONLY place where we compare ALIASES!
        public int GetPropertyIndex(string alias)
        {
            int index;
            if (_indexes.TryGetValue(alias, out index)) return index; // fastest
            if (_indexes.TryGetValue(alias.ToLowerInvariant(), out index)) return index; // slower
            return -1;
        }

        // virtual for unit tests
        public virtual PublishedPropertyType GetPropertyType(string alias)
        {
            var index = GetPropertyIndex(alias);
            return GetPropertyType(index);
        }

        // virtual for unit tests
        public virtual PublishedPropertyType GetPropertyType(int index)
        {
            return index >= 0 && index < _propertyTypes.Length ? _propertyTypes[index] : null;
        }

        #endregion

        
        public static PublishedContentType Get(PublishedItemType itemType, string alias)
        {
            var type = CreatePublishedContentType(itemType, alias);

            return type;
        }

        private static PublishedContentType CreatePublishedContentType(PublishedItemType itemType, string alias)
        {
            if (GetPublishedContentTypeCallback != null)
                return GetPublishedContentTypeCallback(alias);

            IContentTypeComposition contentType;
            switch (itemType)
            {
                case PublishedItemType.Content:
                    contentType = ApplicationContext.Current.Services.ContentTypeService.GetContentType(alias);
                    break;
                case PublishedItemType.Media:
                    contentType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(alias);
                    break;
                case PublishedItemType.Member:
                    contentType = ApplicationContext.Current.Services.MemberTypeService.Get(alias);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("itemType");
            }

            if (contentType == null)
                throw new Exception(string.Format("ContentTypeService failed to find a {0} type with alias \"{1}\".",
                    itemType.ToString().ToLower(), alias));

            return new PublishedContentType(contentType);
        }

        // for unit tests
        internal static Func<string, PublishedContentType> GetPublishedContentTypeCallback { get; set; }
        
    }
}
