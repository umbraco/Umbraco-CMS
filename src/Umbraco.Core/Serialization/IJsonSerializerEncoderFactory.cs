using System.Text.Encodings.Web;

namespace Umbraco.Cms.Core.Serialization;

/// <summary>
/// Provides a factory method for creating a <see cref="JavaScriptEncoder"/> for use in instantiating JSON serializers.
/// </summary>
public interface IJsonSerializerEncoderFactory
{
    /// <summary>
    /// Creates a <see cref="JavaScriptEncoder"/> for use in the serialization of configuration editor JSON.
    /// </summary>
    /// <param name="serializerName">The name of the serializer. If there's a need for different encodings for different serializers, this can be used to distinguish them.</param>
    /// <returns>A <see cref="JavaScriptEncoder"/> instance.</returns>
    JavaScriptEncoder CreateEncoder(string serializerName);
}
