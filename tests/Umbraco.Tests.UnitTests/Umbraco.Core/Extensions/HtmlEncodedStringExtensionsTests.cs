using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

    /// <summary>
    /// Contains unit tests for the HtmlEncodedStringExtensions class, which provides extension methods for HTML-encoded strings.
    /// </summary>
[TestFixture]
public class HtmlEncodedStringExtensionsTests
{
    /// <summary>
    /// Unit test for verifying whether an HTML encoded string is null or consists only of white-space characters, optionally stripping HTML tags before evaluation.
    /// </summary>
    /// <param name="htmlString">The input HTML encoded string to be tested.</param>
    /// <param name="stripHtml">If true, HTML tags are removed from <paramref name="htmlString"/> before checking for white-space.</param>
    /// <param name="expectedResult">The expected boolean result indicating whether the processed string is null or white-space.</param>
    /// <remarks>
    /// This is a parameterized test method using NUnit's <c>[TestCase]</c> attribute.
    /// </remarks>
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
