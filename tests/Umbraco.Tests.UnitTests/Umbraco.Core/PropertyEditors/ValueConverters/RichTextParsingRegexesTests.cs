using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.ValueConverters;

[TestFixture]
public class RichTextParsingRegexesTests
{
    private const string ContentKey = "36cc710a-d8a6-45d0-a07f-7bbd8742cf02";
    private const string LayoutKey = "d2eeef66-4111-42f4-a164-7a523eaffbc2";

    [TestCase($"<umb-rte-block data-key=\"{LayoutKey}\" data-content-key=\"{ContentKey}\"></umb-rte-block>")]
    [TestCase($"<umb-rte-block data-content-key=\"{ContentKey}\" data-key=\"{LayoutKey}\"></umb-rte-block>")]
    [TestCase($"<umb-rte-block data-content-key=\"{ContentKey}\"></umb-rte-block>")]
    [TestCase($"<umb-rte-block class=\"x\" data-content-key=\"{ContentKey}\"></umb-rte-block>")]
    [TestCase($"<umb-rte-block-inline data-key=\"{LayoutKey}\" data-content-key=\"{ContentKey}\"></umb-rte-block-inline>")]
    [TestCase($"<umb-rte-block data-key=\"{LayoutKey}\" data-content-key=\"{ContentKey}\"><!--Umbraco-Block--></umb-rte-block>")]
    public void Can_Match_Block_With_Optional_Attributes(string markup)
    {
        var match = RichTextParsingRegexes.BlockRegex().Match(markup);

        Assert.IsTrue(match.Success);
        Assert.AreEqual(ContentKey, match.Groups["key"].Value);
    }

    [TestCase($"<umb-rte-block data-key=\"{LayoutKey}\"></umb-rte-block>")]
    [TestCase($"<umb-rte-block></umb-rte-block>")]
    public void Cannot_Match_Block_Without_Content_Key(string markup)
    {
        var match = RichTextParsingRegexes.BlockRegex().Match(markup);

        Assert.IsFalse(match.Success);
    }

    [TestCase("umb-rte-block", "")]
    [TestCase("umb-rte-block", $" data-key=\"{LayoutKey}\"")]
    [TestCase("umb-rte-block-inline", $" data-key=\"{LayoutKey}\"")]
    public void Matches_Each_Block_Separately(string tagName, string extraAttributes)
    {
        const string secondContentKey = "6a0a8a2e-6a3b-4d6b-9a3f-2f7f2b5c1d4e";
        var markup =
            $"<p>First</p><{tagName}{extraAttributes} data-content-key=\"{ContentKey}\"><!--Umbraco-Block--></{tagName}>" +
            $"<p>Second</p><{tagName} data-content-key=\"{secondContentKey}\"{extraAttributes}></{tagName}><p>Third</p>";

        var result = RichTextParsingRegexes.BlockRegex().Replace(markup, match => $"[{match.Groups["key"].Value}]");

        Assert.AreEqual($"<p>First</p>[{ContentKey}]<p>Second</p>[{secondContentKey}]<p>Third</p>", result);
    }

    [TestCase($"<umb-rte-block data-content-key=\"{ContentKey}\">some text</umb-rte-block>")]
    [TestCase($"<umb-rte-block data-content-key=\"{ContentKey}\"><p>nested</p></umb-rte-block>")]
    public void Cannot_Match_Block_With_Inner_Content(string markup)
    {
        var match = RichTextParsingRegexes.BlockRegex().Match(markup);

        Assert.IsFalse(match.Success);
    }
}
