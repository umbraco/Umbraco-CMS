using NUnit.Framework;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Persistence.SqlServer;

[TestFixture]
public class TSqlQuotingTests
{
    [Test]
    [TestCase("Umbraco", ExpectedResult = "[Umbraco]")]
    [TestCase("Umbraco-7b1e4f0c", ExpectedResult = "[Umbraco-7b1e4f0c]")]
    [TestCase("", ExpectedResult = "[]")]
    [TestCase("a b", ExpectedResult = "[a b]")]
    public string Can_Quote_An_Identifier_By_Default(string name)
        => TSqlQuoting.QuotedName(name);

    [Test]
    [TestCase('[')]
    [TestCase(']')]
    public void Can_Quote_An_Identifier_With_Either_Bracket(char quote)
        => Assert.AreEqual("[Umbraco]", TSqlQuoting.QuotedName("Umbraco", quote));

    [Test]
    [TestCase("a]b", ExpectedResult = "[a]]b]")]
    [TestCase("a]]b", ExpectedResult = "[a]]]]b]")]
    [TestCase("]", ExpectedResult = "[]]]")]
    public string Can_Escape_A_Closing_Bracket_In_An_Identifier(string name)
        => TSqlQuoting.QuotedName(name);

    [Test]
    [TestCase(@"C:\Data\Umbraco.mdf", ExpectedResult = @"'C:\Data\Umbraco.mdf'")]
    [TestCase("", ExpectedResult = "''")]
    public string Can_Quote_A_String_Literal(string name)
        => TSqlQuoting.QuotedName(name, '\'');

    [Test]
    [TestCase(@"C:\Bob's Site\Umbraco.mdf", ExpectedResult = @"'C:\Bob''s Site\Umbraco.mdf'")]
    [TestCase("''", ExpectedResult = "''''''")]
    [TestCase("'", ExpectedResult = "''''")]
    public string Can_Escape_An_Apostrophe_In_A_String_Literal(string name)
        => TSqlQuoting.QuotedName(name, '\'');

    [Test]
    [TestCase("Umbraco", ExpectedResult = "\"Umbraco\"")]
    [TestCase("a\"b", ExpectedResult = "\"a\"\"b\"")]
    public string Can_Quote_And_Escape_A_Double_Quoted_Name(string name)
        => TSqlQuoting.QuotedName(name, '"');

    // Each quote style escapes only its own delimiter, so a name carrying the others passes through
    // untouched. Escaping the wrong one would corrupt file paths, which legitimately contain both.
    [Test]
    [TestCase("a'b", ExpectedResult = "[a'b]")]
    [TestCase("a\"b", ExpectedResult = "[a\"b]")]
    public string Can_Leave_Other_Delimiters_Alone_When_Quoting_An_Identifier(string name)
        => TSqlQuoting.QuotedName(name);

    [Test]
    [TestCase("a]b", ExpectedResult = "'a]b'")]
    [TestCase("a\"b", ExpectedResult = "'a\"b'")]
    public string Can_Leave_Other_Delimiters_Alone_When_Quoting_A_String_Literal(string name)
        => TSqlQuoting.QuotedName(name, '\'');

    [Test]
    [TestCase('`')]
    [TestCase('(')]
    [TestCase(' ')]
    public void Cannot_Quote_With_An_Unsupported_Character(char quote)
        => Assert.Throws<NotSupportedException>(() => TSqlQuoting.QuotedName("Umbraco", quote));
}
