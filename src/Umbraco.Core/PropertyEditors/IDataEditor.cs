using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data editor.
    /// </summary>
    /// <remarks>This is the base interface for parameter and property editors.</remarks>
    public interface IDataEditor : IDiscoverable
    {
        /// <summary>
        /// Gets the alias of the editor.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets the type of the editor.
        /// </summary>
        /// <remarks>An editor can be a property value editor, or a parameter editor.</remarks>
        EditorType Type { get; }

        /// <summary>
        /// Gets the name of the editor.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the icon of the editor.
        /// </summary>
        /// <remarks>Can be used to display editors when presenting them.</remarks>
        string Icon { get; }

        /// <summary>
        /// Gets the group of the editor.
        /// </summary>
        /// <remarks>Can be used to organize editors when presenting them.</remarks>
        string Group { get; }

        /// <summary>
        /// Gets a value indicating whether the editor is deprecated.
        /// </summary>
        /// <remarks>Deprecated editors are supported but not proposed in the UI.</remarks>
        bool IsDeprecated { get; }

        /// <summary>
        /// Gets a value editor.
        /// </summary>
        IDataValueEditor GetValueEditor(); // fixme - should be configured?!

        /// <summary>
        /// Gets a configured value editor.
        /// </summary>
        IDataValueEditor GetValueEditor(object configuration);

        /// <summary>
        /// Gets the configuration for the value editor.
        /// </summary>
        IDictionary<string, object> DefaultConfiguration { get; }

        /// <summary>
        /// Gets an editor to edit the value editor configuration.
        /// </summary>
        /// <remarks>
        /// <para>Is expected to throw if the editor does not support being configured, e.g. for most parameter editors.</para>
        /// </remarks>
        IConfigurationEditor GetConfigurationEditor();

        /// <summary>
        /// Gets the index value factory for the editor.
        /// </summary>
        IPropertyIndexValueFactory PropertyIndexValueFactory { get; }
    }
}
