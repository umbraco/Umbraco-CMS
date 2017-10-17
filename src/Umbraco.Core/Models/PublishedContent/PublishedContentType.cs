using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

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
        // fixme - benchmark this!
        private readonly Dictionary<string, int> _indexes = new Dictionary<string, int>();

        // fixme used in legacy page.cs that should die
        internal PublishedContentType(IContentType contentType)
            : this(PublishedItemType.Content, contentType, new CurrentPublishedContentTypeFactory())
        { }

        // fixme above and should die
        private class CurrentPublishedContentTypeFactory : IPublishedContentTypeFactory
        {
            public PublishedContentType CreateContentType(PublishedItemType itemType, IContentTypeComposition contentType)
            {
                return new PublishedContentType(itemType, contentType, this);
            }

            public PublishedPropertyType CreatePropertyType(PublishedContentType contentType, PropertyType propertyType)
            {
                return new PublishedPropertyType(contentType, propertyType, Current.PublishedModelFactory, Current.PropertyValueConverters, this);
            }

            public PublishedPropertyType CreatePropertyType(string propertyTypeAlias, int dataTypeId, string editorAlias, bool umbraco = false)
            {
                return new PublishedPropertyType(propertyTypeAlias, dataTypeId, editorAlias, umbraco, Current.PublishedModelFactory, Current.PropertyValueConverters, this);
            }

            public PublishedDataType CreateDataType(int id, string editorAlias)
            {
                return new PublishedDataType(id, editorAlias, new DataTypeConfigurationSource(Current.Services.DataTypeService, Current.PropertyEditors));
            }
        }

        // this is the main and only true ctor
        internal PublishedContentType(PublishedItemType itemType, IContentTypeComposition contentType, IPublishedContentTypeFactory factory)
            : this(contentType.Id, contentType.Alias, itemType, contentType.CompositionAliases())
        {
            var propertyTypes = contentType.CompositionPropertyTypes
                .Select(x => factory.CreatePropertyType(this, x));

            if (itemType == PublishedItemType.Member)
                propertyTypes = WithMemberProperties(this, propertyTypes, factory);
            _propertyTypes = propertyTypes.ToArray();

            InitializeIndexes();
        }

        private PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases)
        {
            Id = id;
            Alias = alias;
            ItemType = itemType;
            CompositionAliases = new HashSet<string>(compositionAliases, StringComparer.InvariantCultureIgnoreCase);
        }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<PublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
            : this(id, alias, PublishedItemType.Content, Enumerable.Empty<string>(), propertyTypes, factory)
        { }

        // internal so it can be used for unit tests
        internal PublishedContentType(int id, string alias, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
            : this(id, alias, PublishedItemType.Content, compositionAliases, propertyTypes, factory)
        { }

        private PublishedContentType(int id, string alias, PublishedItemType itemType, IEnumerable<string> compositionAliases, IEnumerable<PublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
            : this (id, alias, itemType, compositionAliases)
        {
            var propertyTypesA = propertyTypes.ToArray();
            foreach (var propertyType in propertyTypesA)
                propertyType.ContentType = this;

            if (itemType == PublishedItemType.Member)
                propertyTypesA = WithMemberProperties(this, propertyTypesA, factory).ToArray();
            _propertyTypes = propertyTypesA;

            InitializeIndexes();
        }

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
        // not sure it's needed really - this is here for safety purposes
        private static readonly Dictionary<string, Tuple<int, string>> BuiltinMemberProperties = new Dictionary<string, Tuple<int, string>>
        {
            // see also PublishedMember class - exposing special properties as properties
            { "Email", Tuple.Create(Constants.DataTypes.Textbox, Constants.PropertyEditors.TextboxAlias) },
            { "Username", Tuple.Create(Constants.DataTypes.Textbox, Constants.PropertyEditors.TextboxAlias) },
            { "PasswordQuestion", Tuple.Create(Constants.DataTypes.Textbox, Constants.PropertyEditors.TextboxAlias) },
            { "Comments", Tuple.Create(Constants.DataTypes.Textbox, Constants.PropertyEditors.TextboxAlias) },
            { "IsApproved", Tuple.Create(Constants.DataTypes.Boolean, Constants.PropertyEditors.BooleanAlias) },
            { "IsLockedOut", Tuple.Create(Constants.DataTypes.Boolean, Constants.PropertyEditors.BooleanAlias) },
            { "LastLockoutDate", Tuple.Create(Constants.DataTypes.Datetime, Constants.PropertyEditors.DateTimeAlias) },
            { "CreateDate", Tuple.Create(Constants.DataTypes.Datetime, Constants.PropertyEditors.DateTimeAlias) },
            { "LastLoginDate", Tuple.Create(Constants.DataTypes.Datetime, Constants.PropertyEditors.DateTimeAlias) },
            { "LastPasswordChangeDate", Tuple.Create(Constants.DataTypes.Datetime, Constants.PropertyEditors.DateTimeAlias) },
        };

        private static IEnumerable<PublishedPropertyType> WithMemberProperties(PublishedContentType contentType, IEnumerable<PublishedPropertyType> propertyTypes, IPublishedContentTypeFactory factory)
        {
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var propertyType in propertyTypes)
            {
                aliases.Add(propertyType.PropertyTypeAlias);
                yield return propertyType;
            }

            foreach (var propertyType in BuiltinMemberProperties
                .Where(kvp => aliases.Contains(kvp.Key) == false)
                .Select(kvp => factory.CreatePropertyType(kvp.Key, kvp.Value.Item1, kvp.Value.Item2, umbraco: true)))
            {
                // fixme why would it be null?
                if (contentType != null) propertyType.ContentType = contentType;
                yield return propertyType;
            }
        }

        #region Content type

        public int Id { get; }

        public string Alias { get; }

        public PublishedItemType ItemType { get; }

        public HashSet<string> CompositionAliases { get; }

        #endregion

        #region Properties

        public IEnumerable<PublishedPropertyType> PropertyTypes => _propertyTypes;

        // alias is case-insensitive
        // this is the ONLY place where we compare ALIASES!
        public int GetPropertyIndex(string alias)
        {
            if (_indexes.TryGetValue(alias, out var index)) return index; // fastest
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
