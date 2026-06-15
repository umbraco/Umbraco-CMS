// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings.Css;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class StylesheetHelperTests
{
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

        Assert.That(
            result.StripWhitespace(), Is.EqualTo(@"body {font-family:Arial;}/**umb_name:My new rule*/
p{font-size:1em; color:blue;} /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}".StripWhitespace()));
    }

    [Test]
    public void Append_Rule()
    {
        var css =
            @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}";

        var result = StylesheetHelper.AppendRule(
            css,
            new StylesheetRule { Name = "My new rule", Selector = "p", Styles = "font-size:1em; color:blue;" });

        Assert.That(
            result.StripWhitespace(), Is.EqualTo(@"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}

/**umb_name:My new rule*/
p{font-size:1em; color:blue;}".StripWhitespace()));
    }

    [Test]
    public void Duplicate_Names()
    {
        var css = @"/** Umb_Name: Test */ p { font-size: 1em; } /** umb_name:  Test */ li {padding:0px;}";
        var results = StylesheetHelper.ParseRules(css);
        Assert.That(results.Count(), Is.EqualTo(1));
    }

    // Standard rule stle
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
        Assert.That(results.Length, Is.EqualTo(1));

        // Assert.IsTrue(results.First().RuleId.Value.Value.ToString() == file.Id.Value.Value + "/" + name);
        Assert.That(results.First().Name, Is.EqualTo(name));
        Assert.That(results.First().Selector, Is.EqualTo(selector));
        Assert.That(results.First().Styles.StripWhitespace(), Is.EqualTo(styles.StripWhitespace()));
    }

    // No Name: keyword
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
        Assert.That(results.Any(), Is.EqualTo(false));
    }

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
        Assert.That(
            result.NormalizeNewLines(), Is.EqualTo(Tabbed(
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
}").NormalizeNewLines()));
    }

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
        Assert.That(rules.Count(), Is.EqualTo(2));

        Assert.That(rules.First().Name, Is.EqualTo("Test"));
        Assert.That(rules.First().Selector, Is.EqualTo(".test"));
        Assert.That(
            rules.First().Styles.NormalizeNewLines(), Is.EqualTo(@"font-color: red;
margin: 1rem;".NormalizeNewLines()));

        Assert.That(rules.Last().Name, Is.EqualTo("Test2"));
        Assert.That(rules.Last().Selector, Is.EqualTo(".test2"));
        Assert.That(rules.Last().Styles, Is.EqualTo("font-color: green;"));
    }

    // can't put tabs in verbatim strings, so this will replace # with \t to test the CSS indents
    // - and it's tabs because the editor uses tabs, not spaces...
    private static string Tabbed(string input) => input.Replace("#", "\t");
}
