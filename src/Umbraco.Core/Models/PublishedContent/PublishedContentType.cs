using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedElement"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedContentType"/> class are immutable, ie
    /// if the content type changes, then a new class needs to be created.</remarks>
    [DebuggerDisplay("{Alias}")]
    public class PublishedContentType : IPublishedContentType2
    {
        private readonly IPublishedPropertyType[] _propertyTypes;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentType"/> class with a content type.
        /// </summary>
        public PublishedContentType(IContentTypeComposition contentType, IPublishedContentTypeFactory factory)
            : this(contentType.Key, contentType.Id, contentType.Alias, contentType.GetItemType(), contentType.CompositionAliases(), contentType.Variations, contentType.IsElement)
        {
            var propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => factory.CreatePropertyType(this, x))
                .ToList();

            if (ItemType == PublishedItemType.Member)
                EnsureMemberProperties(propertyTypes, factory);

            _propertyTypes = propertyTypes.ToArray();

            InitializeIndexes();
        }

        /// <summary>
        /// This constructor is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>
        /// <para>Values are assumed to be consistent and are not checked.</para>
        /// </remarks>
        public PublishedContentType(Guid key, int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations, bool isElement = false)
            : this(key, id, alias, itemType, compositionAliases, variations, isElement)
        {
            var propertyTypesA = propertyTypes.ToArray();
            foreach (var propertyType in propertyTypesA)
                propertyType.ContentType = this;
            _propertyTypes = propertyTypesA;

            InitializeIndexes();
        }

        [Obsolete("Use the overload specifying a key instead")]
        public PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations, bool isElement = false)
            : this (Guid.Empty, id, alias, itemType, compositionAliases, variations, isElement)
        {
            var propertyTypesA = propertyTypes.ToArray();
            foreach (var propertyType in propertyTypesA)
                propertyType.ContentType = this;
            _propertyTypes = propertyTypesA;

            InitializeIndexes();
        }

        /// <summary>
        /// This constructor is for tests and is not intended to be used directly from application code.
        /// </summary>
        /// <remarks>
        /// <para>Values are assumed to be consistent and are not checked.</para>
        /// </remarks>
        public PublishedContentType(Guid key, int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes, ContentVariation variations, bool isElement = false)
            : this(key, id, alias, itemType, compositionAliases, variations, isElement)
        {
            _propertyTypes = propertyTypes(this).ToArray();

            InitializeIndexes();
        }
        
        [Obsolete("Use the overload specifying a key instead")]
        public PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, Func<IPublishedContentType, IEnumerable<IPublishedPropertyType>> propertyTypes, ContentVariation variations, bool isElement = false)
            : this(Guid.Empty, id, alias, itemType, compositionAliases, variations, isElement)
        {
            _propertyTypes = propertyTypes(this).ToArray();

            InitializeIndexes();
        }

        private PublishedContentType(Guid key, int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, ContentVariation variations, bool isElement)
        {
            Key = key;
            Id = id;
            Alias = alias;
            ItemType = itemType;
            CompositionAliases = new HashSet<string>(compositionAliases, StringComparer.InvariantCultureIgnoreCase);
            Variations = variations;
            IsElement = isElement;
        }

        private void InitializeIndexes()
        {
            for (var i = 0; i < _propertyTypes.Length; i++)
            {
                var propertyType = _propertyTypes[i];
                _indexes[propertyType.Alias] = i;
                _indexes[propertyType.Alias.ToLowerInvariant()] = i;
            }
        }

        // Members have properties such as IMember LastLoginDate which are plain C# properties and not content
        // properties; they are exposed as pseudo content properties, as long as a content property with the
        // same alias does not exist already.
        private void EnsureMemberProperties(List<IPublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
        {
            var aliases = new HashSet<string>(propertyTypes.Select(x => x.Alias), StringComparer.OrdinalIgnoreCase);

            foreach (var (alias, dataTypeId) in BuiltinMemberProperties)
            {
                if (aliases.Contains(alias)) continue;
                propertyTypes.Add(factory.CreateCorePropertyType(this, alias, dataTypeId, ContentVariation.Nothing));
            }
        }

        // TODO: this list somehow also exists in constants, see memberTypeRepository => remove duplicate!
        private static readonly Dictionary<string, int> BuiltinMemberProperties = new Dictionary<string, int>
        {
            { "Email", Constants.DataTypes.Textbox },
            { "Username", Constants.DataTypes.Textbox },
            { "PasswordQuestion", Constants.DataTypes.Textbox },
            { "Comments", Constants.DataTypes.Textarea },
            { "IsApproved", Constants.DataTypes.Boolean },
            { "IsLockedOut", Constants.DataTypes.Boolean },
            { "LastLockoutDate", Constants.DataTypes.LabelDateTime },
            { "CreateDate", Constants.DataTypes.LabelDateTime },
            { "LastLoginDate", Constants.DataTypes.LabelDateTime },
            { "LastPasswordChangeDate", Constants.DataTypes.LabelDateTime }
        };

        #region Content type

        /// <inheritdoc />
        public Guid Key { get; }

        /// <inheritdoc />
        public int Id { get; }

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public PublishedItemType ItemType { get; }

        /// <inheritdoc />
        public HashSet<string> CompositionAliases { get; }

        /// <inheritdoc />
        public ContentVariation Variations { get; }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IEnumerable<IPublishedPropertyType> PropertyTypes => _propertyTypes;

        /// <inheritdoc />
        public int GetPropertyIndex(string alias)
        {
            if (_indexes.TryGetValue(alias, out var index)) return index; // fastest
            if (_indexes.TryGetValue(alias.ToLowerInvariant(), out index)) return index; // slower
            return -1;
        }

        // virtual for unit tests
        // TODO: explain why
        /// <inheritdoc />
        public virtual IPublishedPropertyType GetPropertyType(string alias)
        {
            var index = GetPropertyIndex(alias);
            return GetPropertyType(index);
        }

        // virtual for unit tests
        // TODO: explain why
        /// <inheritdoc />
        public virtual IPublishedPropertyType GetPropertyType(int index)
        {
            return index >= 0 && index < _propertyTypes.Length ? _propertyTypes[index] : null;
        }

        /// <inheritdoc />
        public bool IsElement { get; }

        #endregion
    }
}
