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
    public void Matches_And_Captures_The_Content_Key(string markup)
    {
        var match = RichTextParsingRegexes.BlockRegex().Match(markup);

        Assert.IsTrue(match.Success);
        Assert.AreEqual(ContentKey, match.Groups["key"].Value);
    }
}
