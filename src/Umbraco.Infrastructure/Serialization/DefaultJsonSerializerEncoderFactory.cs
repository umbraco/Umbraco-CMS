using System.Text.Encodings.Web;
using System.Text.Unicode;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class DefaultJsonSerializerEncoderFactory : IJsonSerializerEncoderFactory
{
    /// <inheritdoc />
    public JavaScriptEncoder CreateEncoder<TSerializer>()
        where TSerializer : IJsonSerializer
        => JavaScriptEncoder.Create(UnicodeRanges.BasicLatin);
}
