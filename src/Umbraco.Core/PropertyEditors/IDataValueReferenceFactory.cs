namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a factory for creating <see cref="IDataValueReference"/> instances for specific data editors.
/// </summary>
public interface IDataValueReferenceFactory
{
    /// <summary>
    ///     Gets a value indicating whether the DataValueReference lookup supports a datatype (data editor).
    /// </summary>
    /// <param name="dataEditor">The data editor to check.</param>
    /// <returns>A value indicating whether the converter supports a datatype.</returns>
    bool IsForEditor(IDataEditor? dataEditor);

    /// <summary>
    /// Gets the <see cref="IDataValueReference"/> instance for extracting entity references from property values.
    /// </summary>
    /// <returns>An <see cref="IDataValueReference"/> instance.</returns>
    IDataValueReference GetDataValueReference();
}
