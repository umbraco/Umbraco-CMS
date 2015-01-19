using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Strings.Css;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class StylesheetHelperTests
    {
        [Test]
        public void Replace_Rule()
        {
            var css = @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}";
            var results = StylesheetHelper.ParseRules(css);

            var result = StylesheetHelper.ReplaceRule(css, results.First().Name, new StylesheetRule()
            {
                Name = "My new rule",
                Selector = "p",
                Styles = "font-size:1em; color:blue;"
            });

            Assert.AreEqual(@"body {font-family:Arial;}/**umb_name:My new rule*/
p{font-size:1em; color:blue;} /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}", result);
        }

        [Test]
        public void Append_Rule()
        {
            var css = @"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}";

            var result = StylesheetHelper.AppendRule(css, new StylesheetRule()
            {
                Name = "My new rule",
                Selector = "p",
                Styles = "font-size:1em; color:blue;"
            });

            Assert.AreEqual(@"body {font-family:Arial;}/** Umb_Name: Test1 */ p { font-size: 1em; } /** umb_name:  Test2 */ li {padding:0px;} table {margin:0;}

/**umb_name:My new rule*/
p{font-size:1em; color:blue;}", result);
        }

        [Test]
        public void Duplicate_Names()
        {
            var css = @"/** Umb_Name: Test */ p { font-size: 1em; } /** umb_name:  Test */ li {padding:0px;}";
            var results = StylesheetHelper.ParseRules(css);
            Assert.AreEqual(1, results.Count());
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
        [TestCase("Test", "p", @"font-size: 1em;
color:red; font-weight:bold;

text-align:left;", @"/** umb_name: Test */ p { font-size: 1em; 
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
            var results = StylesheetHelper.ParseRules(css);

            // Assert
            Assert.AreEqual(1, results.Count());

            //Assert.IsTrue(results.First().RuleId.Value.Value.ToString() == file.Id.Value.Value + "/" + name);
            Assert.AreEqual(name, results.First().Name);
            Assert.AreEqual(selector, results.First().Selector);
            Assert.AreEqual(styles, results.First().Styles);
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
        //Only a single asterisk
        [TestCase("/* umb_name: Test */ p { font-size: 1em; }")]
        // Has a name with spaces over multiple lines
        [TestCase(@"/**UMB_NAME:Hello 

world */p{font-size: 1em;}")]
        public void ParseRules_DoesntParse(string css)
        {
         
            // Act
            var results = StylesheetHelper.ParseRules(css);

            // Assert
            Assert.IsTrue(results.Count() == 0);
        }
    }
}
