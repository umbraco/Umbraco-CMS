// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class DictionaryExtensionsTests
{
    [Test]
    public void ToQueryString_Empty_Dictionary_Returns_Empty_String()
    {
        var dictionary = new Dictionary<string, object?>();

        Assert.AreEqual(string.Empty, dictionary.ToQueryString());
    }

    [Test]
    public void ToQueryString_Single_Entry_Has_No_Separator()
    {
        var dictionary = new Dictionary<string, object?> { ["firstname"] = "shannon" };

        Assert.AreEqual("firstname=shannon", dictionary.ToQueryString());
    }

    [Test]
    public void ToQueryString_Multiple_Entries_Are_Ampersand_Separated_Without_Trailing_Ampersand()
    {
        var dictionary = new Dictionary<string, object?>
        {
            ["firstname"] = "shannon",
            ["lastname"] = "deminick",
        };

        var result = dictionary.ToQueryString();

        Assert.Multiple(() =>
        {
            // Dictionary enumeration order is not contractually guaranteed, so assert on the set of pairs.
            Assert.That(result.Split('&'), Is.EquivalentTo(new[] { "firstname=shannon", "lastname=deminick" }));
            Assert.That(result, Does.Not.StartWith("&"));
            Assert.That(result, Does.Not.EndWith("&"));
        });
    }

    [Test]
    public void ToQueryString_Null_Value_Renders_Key_With_Empty_Value()
    {
        var dictionary = new Dictionary<string, object?> { ["key"] = null };

        Assert.AreEqual("key=", dictionary.ToQueryString());
    }

    [Test]
    public void ToQueryString_Encodes_Ampersand_In_Value_And_Does_Not_Trim_It()
    {
        var dictionary = new Dictionary<string, object?> { ["key"] = "a&b" };

        // The '&' inside the value must be URL-encoded (never treated as a separator or trimmed).
        Assert.AreEqual("key=a%26b", dictionary.ToQueryString());
    }
}
