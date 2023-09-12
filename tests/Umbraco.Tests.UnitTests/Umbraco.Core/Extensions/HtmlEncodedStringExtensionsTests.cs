using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class HtmlEncodedStringExtensionsTests
{
    [TestCase(null, false, true)]
    [TestCase("", false, true)]
    [TestCase("   ", false, true)]
    [TestCase("This is a non-empty string", false, false)]
    [TestCase("<p>This is a non-empty string</p>", true,false)]
    [TestCase("<p>This is a non-empty string</p>", false,false)]
    [TestCase("<p></p>", true, true)]
    [TestCase("<p></p>", false, false)]
    public void IsNullOrWhiteSpace(string? htmlString, bool stripHtml, bool expectedResult)
    {
        var htmlEncodedString = htmlString == null ? null : Mock.Of<IHtmlEncodedString>(x => x.ToHtmlString() == htmlString);
        var result = htmlEncodedString.IsNullOrWhiteSpace(stripHtml: stripHtml);

        Assert.AreEqual(expectedResult, result);
    }
}
