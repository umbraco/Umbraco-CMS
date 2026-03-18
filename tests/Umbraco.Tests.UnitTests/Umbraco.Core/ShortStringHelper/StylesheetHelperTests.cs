// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings.Css;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

/// <summary>
/// Contains unit tests for the <see cref="StylesheetHelper"/> class, which is part of the ShortStringHelper functionality in Umbraco.
/// </summary>
[TestFixture]
public class StylesheetHelperTests
{
    /// <summary>
    /// Tests the ReplaceRule method to ensure it correctly replaces a CSS rule in a stylesheet string.
    /// </summary>
    [Test]
    public void Replace_Rule()
    {
        var css =
            @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}";
        var results = StylesheetHelper.ParseRules(css);

        var result = StylesheetHelper.ReplaceRule(
            css,
            results.First().Name,
            new StylesheetRule
            {
                Name = "My new rule",
                Selector = "p",
                Styles = "font-size:1em; color:blue;",
            });

        Assert.AreEqual(
            @"body {font-family:Arial;}/**umb_name:My new rule*/
p{font-size:1em; color:blue;} /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}".StripWhitespace(),
            result.StripWhitespace());
    }

    /// <summary>
    /// Tests the AppendRule method by appending a new CSS rule to an existing stylesheet string and verifying the result.
    /// </summary>
    [Test]
    public void Append_Rule()
    {
        var css =
            @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}";

        var result = StylesheetHelper.AppendRule(
            css,
            new StylesheetRule { Name = "My new rule", Selector = "p", Styles = "font-size:1em; color:blue;" });

        Assert.AreEqual(
            @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}

/**umb_name:My new rule*/
p{font-size:1em; color:blue;}".StripWhitespace(),
            result.StripWhitespace());
    }

    /// <summary>
    /// Tests that duplicate stylesheet names are handled correctly by parsing rules.
    /// </summary>
    [Test]
    public void Duplicate_Names()
    {
        var css = @"/** Umb_Name: Test */ p { font-size: 1em; } /** umb_name:  Test */ li {padding:0px;}";
        var results = StylesheetHelper.ParseRules(css);
        Assert.AreEqual(1, results.Count());
    }

    // Standard rule stle
    /// <summary>
    /// Verifies that <see cref="StylesheetHelper.ParseRules(string)"/> correctly parses a CSS string containing a special Umb_Name comment,
    /// extracting the rule name, selector, and styles as expected.
    /// </summary>
    /// <param name="name">The expected rule name extracted from the Umb_Name comment in the CSS.</param>
    /// <param name="selector">The expected CSS selector for the rule.</param>
    /// <param name="styles">The expected CSS style declarations as a string.</param>
    /// <param name="css">The input CSS string to be parsed.</param>
    [TestCase("Test", "p", "font-size: 1em;", @"/**
    Umb_Name: Test
*/
p {
    font-size: 1em;
}")]

    // All on one line, different casing
    [TestCase("Test", "p", "font-size: 1em;", @"/** Umb_Name: Test */ p { font-size: 1em; }")]

    // styles on several lines
    [TestCase(
        "Test",
        "p",
        @"font-size: 1em;
color:red; font-weight:bold;

text-align:left;",
        @"/** umb_name: Test */ p { font-size: 1em;
color:red; font-weight:bold;

text-align:left;

}")]

    // All on one line with no spaces
    [TestCase("Test", "p", "font-size: 1em;", @"/**UMB_NAME:Test*/p{font-size: 1em;}")]

    // Has a name with spaces
    [TestCase("Hello world", "p", "font-size: 1em;", @"/**UMB_NAME:Hello world */p{font-size: 1em;}")]

    // Every part on a new line
    [TestCase("Test", "p", "font-size: 1em;", @"/**
umb_name:
Test
*/
p
{
font-size: 1em;
}")]
    public void ParseRules_Parses(string name, string selector, string styles, string css)
    {
        // Act
        var results = StylesheetHelper.ParseRules(css).ToArray();

        // Assert
        Assert.AreEqual(1, results.Length);

        // Assert.IsTrue(results.First().RuleId.Value.Value.ToString() == file.Id.Value.Value + "/" + name);
        Assert.AreEqual(name, results.First().Name);
        Assert.AreEqual(selector, results.First().Selector);
        Assert.AreEqual(styles.StripWhitespace(), results.First().Styles.StripWhitespace());
    }

    // No Name: keyword
    /// <summary>
    /// Verifies that the <c>ParseRules</c> method does not parse CSS strings that are either invalid, do not contain a valid <c>umb_name</c> comment, or otherwise do not meet the criteria for rule extraction.
    /// </summary>
    /// <param name="css">A CSS string that is expected to be ignored by the parser.</param>
    [TestCase(@"/** Test2 */
p
{
    font-size: 1em;
}")]

    // Has a Name: keyword, but applies to 2 rules, so shouldn't parse
    [TestCase(@"/** umb_name: Test2 */
p, h2
{
    font-size: 1em;
}")]

    // Has it's name wrapping over two lines
    [TestCase("/** umb_name: Test\r\n2 */ p { font-size: 1em; }")]
    [TestCase("/** umb_name: Test\n2 */ p { font-size: 1em; }")]

    // Only a single asterisk
    [TestCase("/* umb_name: Test */ p { font-size: 1em; }")]

    // Has a name with spaces over multiple lines
    [TestCase(@"/**UMB_NAME:Hello

world */p{font-size: 1em;}")]
    public void ParseRules_DoesntParse(string css)
    {
        // Act
        var results = StylesheetHelper.ParseRules(css);

        // Assert
        Assert.IsTrue(results.Any() == false);
    }

    /// <summary>
    /// Verifies that the <c>AppendRule</c> method correctly appends new CSS rules to existing CSS,
    /// ensuring that formatting such as indentation and special comments (e.g., <c>/**umb_name:...*/</c>)
    /// are properly applied to the resulting stylesheet.
    /// </summary>
    [Test]
    public void AppendRules_IsFormatted()
    {
        // base CSS
        var css = Tabbed(
            @"body {
#font-family:Arial;
}");

        // add a couple of rules
        var result = StylesheetHelper.AppendRule(
            css,
            new StylesheetRule { Name = "Test", Selector = ".test", Styles = "font-color: red;margin: 1rem;" });
        result = StylesheetHelper.AppendRule(
            result,
            new StylesheetRule { Name = "Test2", Selector = ".test2", Styles = "font-color: green;" });

        // verify the CSS formatting including the indents
        Assert.AreEqual(
            Tabbed(
                @"body {
#font-family:Arial;
}

/**umb_name:Test*/
.test {
#font-color: red;
#margin: 1rem;
}

/**umb_name:Test2*/
.test2 {
#font-color: green;
}").NormalizeNewLines(),
            result.NormalizeNewLines());
    }

    /// <summary>
    /// Tests that the ParseRules method can correctly parse formatted CSS rules with custom comments.
    /// </summary>
    [Test]
    public void ParseFormattedRules_CanParse()
    {
        // base CSS
        var css = Tabbed(
            @"body {
#font-family:Arial;
}

/**umb_name:Test*/
.test {
#font-color: red;
#margin: 1rem;
}

/**umb_name:Test2*/
.test2 {
#font-color: green;
}");
        var rules = StylesheetHelper.ParseRules(css).ToArray();
        Assert.AreEqual(2, rules.Count());

        Assert.AreEqual("Test", rules.First().Name);
        Assert.AreEqual(".test", rules.First().Selector);
        Assert.AreEqual(
            @"font-color: red;
margin: 1rem;".NormalizeNewLines(),
            rules.First().Styles.NormalizeNewLines());

        Assert.AreEqual("Test2", rules.Last().Name);
        Assert.AreEqual(".test2", rules.Last().Selector);
        Assert.AreEqual("font-color: green;", rules.Last().Styles);
    }

    // can't put tabs in verbatim strings, so this will replace # with \t to test the CSS indents
    // - and it's tabs because the editor uses tabs, not spaces...
    private static string Tabbed(string input) => input.Replace("#", "\t");
}
