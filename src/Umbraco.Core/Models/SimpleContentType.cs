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

            Id = contentType.Id;
            Key = contentType.Key;
            Alias = contentType.Alias;
            Variations = contentType.Variations;
            Icon = contentType.Icon;
            IsContainer = contentType.IsContainer;
            Name = contentType.Name;
            AllowedAsRoot = contentType.AllowedAsRoot;
            IsElement = contentType.IsElement;
        }

        /// <inheritdoc />
        public string Alias { get; }

        public int Id { get; }

        public Guid Key { get; }

        /// <inheritdoc />
        public ITemplate DefaultTemplate { get;  }

        /// <inheritdoc />
        public ContentVariation Variations { get; }

        /// <inheritdoc />
        public string Icon { get; }

        /// <inheritdoc />
        public bool IsContainer { get; }
        
        public string Name { get; }

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

        string ITreeEntity.Name { get => this.Name; set => throw new NotImplementedException(); }
        int IEntity.Id { get => this.Id; set => throw new NotImplementedException(); }
        bool IEntity.HasIdentity => this.Id != default;
        Guid IEntity.Key { get => this.Key; set => throw new NotImplementedException(); }

        int ITreeEntity.CreatorId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int ITreeEntity.ParentId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int ITreeEntity.Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string ITreeEntity.Path { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        int ITreeEntity.SortOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        bool ITreeEntity.Trashed => throw new NotImplementedException();        
        DateTime IEntity.CreateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DateTime IEntity.UpdateDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        DateTime? IEntity.DeleteDate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        void ITreeEntity.SetParent(ITreeEntity parent) => throw new NotImplementedException();
        object IDeepCloneable.DeepClone() => throw new NotImplementedException();
        bool IRememberBeingDirty.WasDirty() => throw new NotImplementedException();
        bool IRememberBeingDirty.WasPropertyDirty(string propertyName) => throw new NotImplementedException();
        void IRememberBeingDirty.ResetWereDirtyProperties() => throw new NotImplementedException();
        void IRememberBeingDirty.ResetDirtyProperties(bool rememberDirty) => throw new NotImplementedException();
        IEnumerable<string> IRememberBeingDirty.GetWereDirtyProperties() => throw new NotImplementedException();
        bool ICanBeDirty.IsDirty() => throw new NotImplementedException();
        bool ICanBeDirty.IsPropertyDirty(string propName) => throw new NotImplementedException();
        IEnumerable<string> ICanBeDirty.GetDirtyProperties() => throw new NotImplementedException();
        void ICanBeDirty.ResetDirtyProperties() => throw new NotImplementedException();
    }
}
