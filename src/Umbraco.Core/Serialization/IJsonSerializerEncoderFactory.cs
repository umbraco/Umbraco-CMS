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
    /// <typeparam name="TSerializer">The type of the serializer for which the encoder is being created.</typeparam>
    /// <returns>A <see cref="JavaScriptEncoder"/> instance.</returns>
    JavaScriptEncoder CreateEncoder<TSerializer>()
        where TSerializer : IJsonSerializer;
}
