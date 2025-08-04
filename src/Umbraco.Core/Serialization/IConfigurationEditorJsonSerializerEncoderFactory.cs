using System.Text.Encodings.Web;

namespace Umbraco.Cms.Core.Serialization;

/// <summary>
/// Provides a factory method for creating a <see cref="JavaScriptEncoder"/> for use in the serialization of configuration editor JSON.
/// </summary>
public interface IConfigurationEditorJsonSerializerEncoderFactory
{
    /// <summary>
    /// Creates a <see cref="JavaScriptEncoder"/> for use in the serialization of configuration editor JSON.
    /// </summary>
    /// <returns>A <see cref="JavaScriptEncoder"/> instance.</returns>
    JavaScriptEncoder CreateEncoder();
}
