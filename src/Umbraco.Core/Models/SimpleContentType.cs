using System;

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
        {
            if (contentType == null) throw new ArgumentNullException(nameof(contentType));


            Id = contentType.Id;
            Alias = contentType.Alias;
            DefaultTemplate = contentType.DefaultTemplate;
            Variations = contentType.Variations;
            Icon = contentType.Icon;
            IsContainer = contentType.IsContainer;
            Icon = contentType.Icon;
            Name = contentType.Name;
            AllowedAsRoot = contentType.AllowedAsRoot;
            IsElement = contentType.IsElement;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentType"/> class.
        /// </summary>
        public SimpleContentType(IMediaType mediaType)
        {
            if (mediaType == null) throw new ArgumentNullException(nameof(mediaType));

            Id = mediaType.Id;
            Alias = mediaType.Alias;
            Variations = mediaType.Variations;
            Icon = mediaType.Icon;
            IsContainer = mediaType.IsContainer;
            Icon = mediaType.Icon;
            Name = mediaType.Name;
            AllowedAsRoot = mediaType.AllowedAsRoot;
            IsElement = mediaType.IsElement;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentType"/> class.
        /// </summary>
        public SimpleContentType(IMemberType memberType)
        {
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));

            Id = memberType.Id;
            Alias = memberType.Alias;
            Variations = memberType.Variations;
            Icon = memberType.Icon;
            IsContainer = memberType.IsContainer;
            Icon = memberType.Icon;
            Name = memberType.Name;
            AllowedAsRoot = memberType.AllowedAsRoot;
            IsElement = memberType.IsElement;
        }

        /// <inheritdoc />
        public string Alias { get; }

        /// <inheritdoc />
        public int Id { get;  }

        /// <inheritdoc />
        public ITemplate DefaultTemplate { get;  }

        /// <inheritdoc />
        public ContentVariation Variations { get; }

        /// <inheritdoc />
        public string Icon { get; }

        /// <inheritdoc />
        public bool IsContainer { get; }

        /// <inheritdoc />
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
    }
}
