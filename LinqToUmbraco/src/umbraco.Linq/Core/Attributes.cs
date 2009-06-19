using System;

namespace umbraco.Linq.Core
{
    /// <summary>
    /// Standard umbraco info
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class UmbracoInfoAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoInfoAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias for this piece of umbraco info.</param>
        public UmbracoInfoAttribute(string alias)
        {
            this.Alias = alias;
        }

        /// <summary>
        /// Gets or sets the display name of the item.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName { get; set; }
        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>The alias.</value>
        public string Alias { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether this property is mandatory.
        /// </summary>
        /// <value><c>true</c> if mandatory; otherwise, <c>false</c>.</value>
        public bool Mandatory { get; set; }
        
    }

    /// <summary>
    /// Marks a class as an umbraco DocType
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class DocTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeAttribute"/> class.
        /// </summary>
        public DocTypeAttribute()
        {
        }
    }

    /// <summary>
    /// Marks a property as a standard umbraco field
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// - ID
    /// - ParentID
    /// - CreateDate
    /// </remarks>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class FieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute"/> class.
        /// </summary>
        public FieldAttribute()
        {
        }
    }

    /// <summary>
    /// Marks a property as a custom umbraco DocType property
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class PropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAttribute"/> class.
        /// </summary>
        public PropertyAttribute()
        {
        }
    }
}
