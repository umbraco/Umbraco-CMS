using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a ContentType, which Content is based on
    /// </summary>
    public interface ISimpleContentType
    {
        string Alias { get; }
        int Id { get; }
        ITemplate DefaultTemplate { get; }
        ContentVariation Variations { get; }
        string Icon { get; }
        bool IsContainer { get; }
        string Name { get; }
        bool AllowedAsRoot { get; }
        bool SupportsPropertyVariation(string culture, string s, bool b);
    }

    public class SimpleContentType : ISimpleContentType
    {
        public SimpleContentType(IContentType contentType)
        {
            Id = contentType.Id;
            Alias = contentType.Alias;
            DefaultTemplate = contentType.DefaultTemplate;
            Variations = contentType.Variations;
            Icon = contentType.Icon;
            IsContainer = contentType.IsContainer;
            Icon = contentType.Icon;
            Name = contentType.Name;
            AllowedAsRoot = contentType.AllowedAsRoot;
        }

        public string Alias { get; }
        public int Id { get;  }
        public ITemplate DefaultTemplate { get;  }
        public ContentVariation Variations { get; }
        public string Icon { get; }
        public bool IsContainer { get; }
        public string Name { get; }
        public bool AllowedAsRoot { get; }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleContentType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Alias != null ? Alias.GetHashCode() : 0) * 397) ^ Id;
            }
        }
    }
}
