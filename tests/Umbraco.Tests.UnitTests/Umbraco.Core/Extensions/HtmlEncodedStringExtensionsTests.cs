using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class HtmlEncodedStringExtensionsTests
{
    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("   ", true)]
    [TestCase("This is a non-empty string", false)]
    [TestCase("<p>This is a non-empty string</p>", false)]
    [TestCase("<p></p>", true)]
    public void IsNullOrWhiteSpace(string? htmlString, bool expectedResult)
    {
        var htmlEncodedString = htmlString == null ? null : Mock.Of<IHtmlEncodedString>(x => x.ToHtmlString() == htmlString);
        var result = htmlEncodedString.IsNullOrWhiteSpace(stripHtml: true);

        Assert.AreEqual(expectedResult, result);
    }
}
