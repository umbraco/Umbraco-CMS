namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Specifies the different types of property cacheable values.
    /// </summary>
    public enum PropertyCacheValue
    {
        /// <summary>
        /// All of them.
        /// </summary>
        All,

        /// <summary>
        /// The source value ie the internal value that can be used to create both the
        /// object value and the xpath value.
        /// </summary>
        Source,

        /// <summary>
        /// The object value ie the strongly typed value of the property as seen when accessing content via C#.
        /// </summary>
        Object,

        /// <summary>
        /// The XPath value ie the value of the property as seen when accessing content via XPath.
        /// </summary>
        XPath
    }
}
