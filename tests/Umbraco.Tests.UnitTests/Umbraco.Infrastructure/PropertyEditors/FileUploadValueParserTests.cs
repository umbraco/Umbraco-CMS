// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class FileUploadValueParserTests
{
    private static SystemTextJsonSerializer CreateSerializer() => new(new DefaultJsonSerializerEncoderFactory());

    private static FileUploadValueParser CreateParser() => new(CreateSerializer());

    [Test]
    public void Cannot_Parse_Null_Input()
        => Assert.IsNull(CreateParser().Parse(null));

    [Test]
    public void Can_Parse_Plain_String_As_Src()
    {
        FileUploadValue? result = CreateParser().Parse("media/abc/image.jpg");

        Assert.IsNotNull(result);
        Assert.AreEqual("media/abc/image.jpg", result!.Src);
        Assert.IsNull(result.TemporaryFileId);
    }

    [Test]
    public void Can_Parse_Json_Value()
    {
        var serializer = CreateSerializer();
        var parser = new FileUploadValueParser(serializer);
        var temporaryFileId = Guid.NewGuid();
        var json = serializer.Serialize(new FileUploadValue { Src = "media/abc/image.jpg", TemporaryFileId = temporaryFileId });

        FileUploadValue? result = parser.Parse(json);

        Assert.IsNotNull(result);
        Assert.AreEqual("media/abc/image.jpg", result!.Src);
        Assert.AreEqual(temporaryFileId, result.TemporaryFileId);
    }
}
