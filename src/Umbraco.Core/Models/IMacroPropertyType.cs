namespace Umbraco.Core.Models
{
    public interface IMacroPropertyType
    {
        /// <summary>
        /// Identifier
        /// </summary>
        int Id { get; }

        /// <summary>
        /// The alias of the MacroPropertyType
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// The assembly (without the .dll extension) used to retrieve the component at runtime
        /// </summary>
        string Assembly { get; }

        /// <summary>
        /// The MacroPropertyType
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The IMacroGuiRendering component (namespace.namespace.Classname)
        /// </summary>
        string BaseType { get; }
    }
}
