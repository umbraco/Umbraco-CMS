namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Represents a strongly-typed property set.
    /// </summary>
    /// <remarks>Every strongly-typed property set class should inherit from <c>PropertySetModel</c>
    /// (or inherit from a class that inherits from... etc.) so they are picked by the factory.</remarks>
    public class PropertySetModel : PropertySetWrapped
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetModel"/> class with
        /// an original <see cref="IPropertySet"/> instance.
        /// </summary>
        /// <param name="content">The original content.</param>
        protected PropertySetModel(IPropertySet content)
            : base(content)
        { }
    }
}
