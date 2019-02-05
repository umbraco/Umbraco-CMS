using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Implements <see cref="ISimpleContentType"/>.
    /// </summary>
    public class SimpleContentType : ISimpleContentType
    {
        private readonly int _id;
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentType"/> class.
        /// </summary>
        public SimpleContentType(IContentType contentType)
            : this((IContentTypeBase)contentType)
        {
            DefaultTemplate = contentType.DefaultTemplate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentType"/> class.
        /// </summary>
        public SimpleContentType(IMediaType mediaType)
            : this((IContentTypeBase)mediaType)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentType"/> class.
        /// </summary>
        public SimpleContentType(IMemberType memberType)
            : this((IContentTypeBase)memberType)
        { }

        private SimpleContentType(IContentTypeBase contentType)
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));

            _id = contentType.Id;
            Alias = contentType.Alias;
            Variations = contentType.Variations;
            Icon = contentType.Icon;
            IsContainer = contentType.IsContainer;
            Icon = contentType.Icon;
            _name = contentType.Name;
            AllowedAsRoot = contentType.AllowedAsRoot;
            IsElement = contentType.IsElement;
        }

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public int Id
        {
            get => _id;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public ITemplate DefaultTemplate { get;  }

        /// <inheritdoc />
        public ContentVariation Variations { get; }

        /// <inheritdoc />
        public string Icon { get; }

        /// <inheritdoc />
        public bool IsContainer { get; }

        /// <inheritdoc />
        public string Name
        {
            get => _name;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool AllowedAsRoot { get; }

        /// <inheritdoc />
        public bool IsElement { get; }

        /// <inheritdoc />
        public bool SupportsPropertyVariation(string culture, string segment, bool wildcards = false)
        {
            // non-exact validation: can accept a 'null' culture if the property type varies
            //  by culture, and likewise for segment
            // wildcard validation: can accept a '*' culture or segment
            return Variations.ValidateVariation(culture, segment, false, wildcards, false);
        }

        protected bool Equals(SimpleContentType other)
        {
            return string.Equals(Alias, other.Alias) && Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SimpleContentType) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Alias != null ? Alias.GetHashCode() : 0) * 397) ^ Id;
            }
        }

        // we have to have all this, because we're an IUmbracoEntity, because that is
        // required by the query expression visitor / SimpleContentTypeMapper

        public object DeepClone() => throw new NotImplementedException();

        public Guid Key { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime UpdateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime? DeleteDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool HasIdentity => throw new NotImplementedException();

        public int CreatorId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ParentId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public void SetParent(ITreeEntity parent) => throw new NotImplementedException();

        public int Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int SortOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Trashed => throw new NotImplementedException();
        public bool IsDirty() => throw new NotImplementedException();
        public bool IsPropertyDirty(string propName) => throw new NotImplementedException();
        public IEnumerable<string> GetDirtyProperties() => throw new NotImplementedException();
        public void ResetDirtyProperties() => throw new NotImplementedException();
        public bool WasDirty() => throw new NotImplementedException();
        public bool WasPropertyDirty(string propertyName) => throw new NotImplementedException();
        public void ResetWereDirtyProperties() => throw new NotImplementedException();
        public void ResetDirtyProperties(bool rememberDirty) => throw new NotImplementedException();
        public IEnumerable<string> GetWereDirtyProperties() => throw new NotImplementedException();
    }
}
