using System.Text.Encodings.Web;
using System.Text.Unicode;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <inheritdoc />
public sealed class DefaultConfigurationEditorJsonSerializerEncoderFactory : IConfigurationEditorJsonSerializerEncoderFactory
{
    /// <inheritdoc />
    public JavaScriptEncoder CreateEncoder() => JavaScriptEncoder.Create(UnicodeRanges.BasicLatin);
}
