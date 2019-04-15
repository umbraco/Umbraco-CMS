using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents an <see cref="IPublishedElement"/> type.
    /// </summary>
    /// <remarks>Instances of the <see cref="PublishedContentType"/> class are immutable, ie
    /// if the content type changes, then a new class needs to be created.</remarks>
    public class PublishedContentType : IPublishedContentType
    {
        private readonly PublishedPropertyType[] _propertyTypes;

        // fast alias-to-index xref containing both the raw alias and its lowercase version
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentType"/> class with a content type.
        /// </summary>
        public PublishedContentType(IContentTypeComposition contentType, IPublishedContentTypeFactory factory)
            : this(contentType.Id, contentType.Alias, contentType.GetItemType(), contentType.CompositionAliases(), contentType.Variations, contentType.IsElement)
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
        /// Initializes a new instance of the <see cref="PublishedContentType"/> with specific values.
        /// </summary>
        /// <remarks>
        /// <para>This constructor is for tests and is not intended to be used directly from application code.</para>
        /// <para>Values are assumed to be consisted and are not checked.</para>
        /// </remarks>
        public PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, ContentVariation variations, bool isElement = false)
            : this (id, alias, itemType, compositionAliases, variations, isElement)
        {
            var propertyTypesA = propertyTypes.ToArray();
            foreach (var propertyType in propertyTypesA)
                propertyType.ContentType = this;
            _propertyTypes = propertyTypesA;

            InitializeIndexes();
        }

        private PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, ContentVariation variations, bool isElement)
        {
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
        private void EnsureMemberProperties(List<PublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
        {
            var aliases = new HashSet<string>(propertyTypes.Select(x => x.Alias), StringComparer.OrdinalIgnoreCase);

            foreach ((var alias, (var dataTypeId, var editorAlias)) in BuiltinMemberProperties)
            {
                if (aliases.Contains(alias)) continue;
                propertyTypes.Add(factory.CreatePropertyType(this, alias, dataTypeId, ContentVariation.Nothing));
            }
        }

        // TODO: this list somehow also exists in constants, see memberTypeRepository => remove duplicate!
        private static readonly Dictionary<string, (int, string)> BuiltinMemberProperties = new Dictionary<string, (int, string)>
        {
            { "Email", (Constants.DataTypes.Textbox, Constants.PropertyEditors.Aliases.TextBox) },
            { "Username", (Constants.DataTypes.Textbox, Constants.PropertyEditors.Aliases.TextBox) },
            { "PasswordQuestion", (Constants.DataTypes.Textbox, Constants.PropertyEditors.Aliases.TextBox) },
            { "Comments", (Constants.DataTypes.Textbox, Constants.PropertyEditors.Aliases.TextBox) },
            { "IsApproved", (Constants.DataTypes.Boolean, Constants.PropertyEditors.Aliases.Boolean) },
            { "IsLockedOut", (Constants.DataTypes.Boolean, Constants.PropertyEditors.Aliases.Boolean) },
            { "LastLockoutDate", (Constants.DataTypes.DateTime, Constants.PropertyEditors.Aliases.DateTime) },
            { "CreateDate", (Constants.DataTypes.DateTime, Constants.PropertyEditors.Aliases.DateTime) },
            { "LastLoginDate", (Constants.DataTypes.DateTime, Constants.PropertyEditors.Aliases.DateTime) },
            { "LastPasswordChangeDate", (Constants.DataTypes.DateTime, Constants.PropertyEditors.Aliases.DateTime) },
        };

        #region Content type

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
        public IEnumerable<PublishedPropertyType> PropertyTypes => _propertyTypes;

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
        public virtual PublishedPropertyType GetPropertyType(string alias)
        {
            var index = GetPropertyIndex(alias);
            return GetPropertyType(index);
        }

        // virtual for unit tests
        // TODO: explain why
        /// <inheritdoc />
        public virtual PublishedPropertyType GetPropertyType(int index)
        {
            return index >= 0 && index < _propertyTypes.Length ? _propertyTypes[index] : null;
        }

        /// <inheritdoc />
        public bool IsElement { get; }

        #endregion
    }
}
