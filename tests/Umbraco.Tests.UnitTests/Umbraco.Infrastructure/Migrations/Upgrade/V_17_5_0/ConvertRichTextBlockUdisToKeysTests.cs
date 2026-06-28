// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_5_0;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations.Upgrade.V_17_5_0;

[TestFixture]
public class ConvertRichTextBlockUdisToKeysTests
{
    private const string LegacyKey = "ccda6d01e3cd44dd883fdf9f5c84d3a7";
    private const string DashedKey = "ccda6d01-e3cd-44dd-883f-df9f5c84d3a7";

    // The JSON unicode escape for a double quote: backslash + "u0022". Built with a doubled backslash so the
    // literal six-character sequence ends up in the string.
    private const string U = "\\u0022";        // "   (one nesting level)
    private const string Uu = "\\\\u0022";      // \\u0022  (nested one level deeper)

    private static string Convert(string value) => ConvertRichTextBlockUdisToKeys.ConvertLegacyBlockReferences(value);

    [Test]
    public void Converts_Inline_Block_Reference()
    {
        var input = $"<p><umb-rte-block-inline data-content-udi=\"umb://element/{LegacyKey}\"><!--Umbraco-Block--></umb-rte-block-inline></p>";
        var expected = $"<p><umb-rte-block-inline data-content-key=\"{DashedKey}\"><!--Umbraco-Block--></umb-rte-block-inline></p>";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Converts_Block_Level_Reference()
    {
        var input = $"<umb-rte-block data-content-udi=\"umb://element/{LegacyKey}\"><!--Umbraco-Block--></umb-rte-block>";
        var expected = $"<umb-rte-block data-content-key=\"{DashedKey}\"><!--Umbraco-Block--></umb-rte-block>";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Converts_Reference_With_Class_Attribute_Preserved()
    {
        var input = $"<umb-rte-block-inline class=\"foo\" data-content-udi=\"umb://element/{LegacyKey}\"></umb-rte-block-inline>";
        var expected = $"<umb-rte-block-inline class=\"foo\" data-content-key=\"{DashedKey}\"></umb-rte-block-inline>";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Converts_All_References_In_Value()
    {
        var input =
            "<umb-rte-block-inline data-content-udi=\"umb://element/7b28c91c65324912b9f799df6a52fc65\"></umb-rte-block-inline>" +
            "some text" +
            "<umb-rte-block-inline data-content-udi=\"umb://element/9134954467c44e4ca6c45b8d9392c707\"></umb-rte-block-inline>";

        var result = Convert(input);

        Assert.Multiple(() =>
        {
            Assert.That(result, Does.Contain("data-content-key=\"7b28c91c-6532-4912-b9f7-99df6a52fc65\""));
            Assert.That(result, Does.Contain("data-content-key=\"91349544-67c4-4e4c-a6c4-5b8d9392c707\""));
            Assert.That(result, Does.Not.Contain("data-content-udi"));
        });
    }

    [Test]
    public void Preserves_Single_Level_Json_Escaping()
    {
        // As stored directly in umbracoPropertyData.textValue: the markup lives inside a JSON string, so the
        // attribute quotes are escaped once (\").
        var input = $@"{{""markup"":""<p><umb-rte-block-inline data-content-udi=\""umb://element/{LegacyKey}\""></umb-rte-block-inline></p>""}}";
        var expected = $@"{{""markup"":""<p><umb-rte-block-inline data-content-key=\""{DashedKey}\""></umb-rte-block-inline></p>""}}";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Preserves_Nested_Double_Level_Json_Escaping()
    {
        // When a Rich Text Editor is nested inside another editor (e.g. a Block List) its markup is escaped
        // twice (\\\"). The leading backslash run must be preserved identically on both quotes.
        var input = $@"data-content-udi=\\\""umb://element/{LegacyKey}\\\""";
        var expected = $@"data-content-key=\\\""{DashedKey}\\\""";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Converts_When_Quote_Is_Unicode_Escaped()
    {
        // Real-world form: the attribute quote is serialized as the JSON unicode escape (backslash u0022)
        // rather than as an escaped double-quote.
        var input = $"<umb-rte-block-inline data-content-udi={U}umb://element/{LegacyKey}{U}></umb-rte-block-inline>";
        var expected = $"<umb-rte-block-inline data-content-key={U}{DashedKey}{U}></umb-rte-block-inline>";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Converts_When_Quote_Is_Nested_Unicode_Escaped()
    {
        // Nested one level deeper: the unicode-escaped quote gains an extra escaping backslash.
        var input = $"data-content-udi={Uu}umb://element/{LegacyKey}{Uu}";
        var expected = $"data-content-key={Uu}{DashedKey}{Uu}";

        Assert.AreEqual(expected, Convert(input));
    }

    [Test]
    public void Leaves_Malformed_Identifier_Untouched()
    {
        const string input = "<umb-rte-block-inline data-content-udi=\"umb://element/not-a-guid\"></umb-rte-block-inline>";

        Assert.AreEqual(input, Convert(input));
    }

    [Test]
    public void Is_Idempotent()
    {
        var alreadyMigrated = $"<umb-rte-block-inline data-content-key=\"{DashedKey}\"></umb-rte-block-inline>";

        Assert.AreEqual(alreadyMigrated, Convert(alreadyMigrated));
    }

    [Test]
    public void Leaves_Value_Without_Block_References_Untouched()
    {
        const string input = "<p>Just some <strong>rich</strong> text with no blocks.</p>";

        Assert.AreEqual(input, Convert(input));
    }
}
