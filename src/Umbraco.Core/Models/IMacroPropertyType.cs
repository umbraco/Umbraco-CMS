namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a PropertyType (plugin) for a Macro
    /// </summary>
    internal interface IMacroPropertyType
    {
        /// <summary>
        /// Gets the unique Alias of the Property Type
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets the name of the Assembly used to render the Property Type
        /// </summary>
        string RenderingAssembly { get; }
        
        /// <summary>
        /// Gets the name of the Type used to render the Property Type
        /// </summary>
        string RenderingType { get; }
        
        /// <summary>
        /// Gets the Base Type for storing the PropertyType (Int32, String, Boolean)
        /// </summary>
        MacroPropertyTypeBaseTypes BaseType { get; }
    }
}