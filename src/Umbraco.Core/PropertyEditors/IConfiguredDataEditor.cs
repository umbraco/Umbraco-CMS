namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a data editor which can be configured.
    /// </summary>
    public interface IConfiguredDataEditor : IDataEditor
    {
        /// <summary>
        /// Gets the editor to edit the value editor configuration.
        /// </summary>
        IConfigurationEditor ConfigurationEditor { get; } // fixme should be a method - but, deserialization?
    }
}