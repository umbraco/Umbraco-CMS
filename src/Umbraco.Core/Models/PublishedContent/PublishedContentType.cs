using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly HashSet<string> _compositionAliases;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        // internal so it can be used by PublishedNoCache which does _not_ want to cache anything and so will never
        // use the static cache getter PublishedContentType.GetPublishedContentType(alias) below - anything else
        // should use it.
        internal PublishedContentType(IContentType contentType)
            : this(PublishedItemType.Content, contentType)
        { }

        internal PublishedContentType(IMediaType mediaType)
            : this(PublishedItemType.Media, mediaType)
        { }

        internal PublishedContentType(IMemberType memberType)
            : this(PublishedItemType.Member, memberType)
        { }

        internal PublishedContentType(PublishedItemType itemType, IContentTypeComposition contentType)
        {
            Id = contentType.Id;
            Alias = contentType.Alias;
            ItemType = itemType;
            _compositionAliases = new HashSet<string>(contentType.CompositionAliases(), StringComparer.InvariantCultureIgnoreCase);
            var propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => new PublishedPropertyType(this, x));
            if (itemType == PublishedItemType.Member)
                propertyTypes = WithMemberProperties(propertyTypes, this);
            _propertyTypes = propertyTypes.ToArray();
            InitializeIndexes();
        }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes)
            : this(id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes)
        { }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : this(id, alias, PublishedItemType.Content, compositionAliases, propertyTypes)
        { }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
        {
            Id = id;
            Alias = alias;
            ItemType = itemType;
            _compositionAliases = new HashSet<string>(compositionAliases, StringComparer.InvariantCultureIgnoreCase);
            if (itemType == PublishedItemType.Member)
                propertyTypes = WithMemberProperties(propertyTypes);
            _propertyTypes = propertyTypes.ToArray();
            foreach (var propertyType in _propertyTypes)
                propertyType.ContentType = this;
            InitializeIndexes();
        }

        // create detached content type - ie does not match anything in the DB
        internal PublishedContentType(string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes)
            : this(0, alias, compositionAliases, propertyTypes)
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

        // NOTE: code below defines and add custom, built-in, Umbraco properties for members
        //  unless they are already user-defined in the content type, then they are skipped

        // fixme should have constants for these
        private const int TextboxDataTypeDefinitionId = -88;
        //private const int BooleanDataTypeDefinitionId = -49;
        //private const int DatetimeDataTypeDefinitionId = -36;

        static readonly Dictionary<string, Tuple<int, string>> BuiltinProperties = new Dictionary<string, Tuple<int, string>>
        {
            // fixme is this ok?
            { "Email", Tuple.Create(TextboxDataTypeDefinitionId, Constants.PropertyEditors.TextboxAlias) },
            { "Username", Tuple.Create(TextboxDataTypeDefinitionId, Constants.PropertyEditors.TextboxAlias) },
            //{ "PasswordQuestion", Tuple.Create(TextboxDataTypeDefinitionId, Constants.PropertyEditors.TextboxAlias) },
            //{ "Comments", Tuple.Create(TextboxDataTypeDefinitionId, Constants.PropertyEditors.TextboxAlias) },
            //{ "IsApproved", Tuple.Create(BooleanDataTypeDefinitionId, Constants.PropertyEditors.BooleanEditorAlias) },
            //{ "IsLockedOut", Tuple.Create(BooleanDataTypeDefinitionId, Constants.PropertyEditors.BooleanEditorAlias) },
            //{ "LastLockoutDate", Tuple.Create(DatetimeDataTypeDefinitionId, Constants.PropertyEditors.DatetimeEditorAlias) },
            //{ "CreateDate", Tuple.Create(DatetimeDataTypeDefinitionId, Constants.PropertyEditors.DatetimeEditorAlias) },
            //{ "LastLoginDate", Tuple.Create(DatetimeDataTypeDefinitionId, Constants.PropertyEditors.DatetimeEditorAlias) },
            //{ "LastPasswordChangeDate", Tuple.Create(DatetimeDataTypeDefinitionId, Constants.PropertyEditors.DatetimeEditorAlias) },
        };

        private static IEnumerable<PublishedPropertyType> WithMemberProperties(IEnumerable<PublishedPropertyType> propertyTypes,
            PublishedContentType contentType = null)
        {
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var propertyType in propertyTypes)
            {
                aliases.Add(propertyType.PropertyTypeAlias);
                yield return propertyType;
            }

            foreach (var kvp in BuiltinProperties.Where(kvp => aliases.Contains(kvp.Key) == false))
            {
                var propertyType = new PublishedPropertyType(kvp.Key, kvp.Value.Item1, kvp.Value.Item2, true);
                if (contentType != null) propertyType.ContentType = contentType;
                yield return propertyType;
            }
        }

        #region Content type

        public int Id { get; private set; }

        public string Alias { get; private set; }

        public PublishedItemType ItemType { get; private set; }

        public HashSet<string> CompositionAliases => _compositionAliases;

        #endregion

        #region Properties

        public IEnumerable<PublishedPropertyType> PropertyTypes => _propertyTypes;

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
    }
}
