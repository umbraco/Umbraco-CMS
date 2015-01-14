using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core.Strings.Css;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class StylesheetHelperTests
    {
        // Standard rule stle
        [TestCase("Test", "p", "font-size: 1em;", @"/*
    Name: Test
*/
p {
    font-size: 1em;
}")]
        // All on one line
        [TestCase("Test", "p", "font-size: 1em;", @"/* Name: Test */ p { font-size: 1em; }")]
        // styles on several lines
        [TestCase("Test", "p", @"font-size: 1em;
color:red; font-weight:bold;

text-align:left;", @"/* Name: Test */ p { font-size: 1em; 
color:red; font-weight:bold;

text-align:left; 

}")]
        // All on one line with no spaces
        [TestCase("Test", "p", "font-size: 1em;", @"/*Name:Test*/p{font-size: 1em;}")]
        // Every part on a new line
        [TestCase("Test", "p", "font-size: 1em;", @"/* 
Name:
Test
*/
p
{
font-size: 1em;
}")]
        public void StylesheetHelperTests_ParseRules_Parses(string name, string selector, string styles, string css)
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
        [TestCase(@"/* Test2 */
p
{
    font-size: 1em;
}")]
        // Has a Name: keyword, but applies to 2 rules, so shouldn't parse
        [TestCase(@"/* Name: Test2 */
p, h2
{
    font-size: 1em;
}")]
        // Has it's name wrapping over two lines
        [TestCase("/* Name: Test\r\n2 */ p { font-size: 1em; }")]
        [TestCase("/* Name: Test\n2 */ p { font-size: 1em; }")]
        public void StylesheetHelperTests_ParseRules_DoesntParse(string css)
        {
         
            // Act
            var results = StylesheetHelper.ParseRules(css);

            // Assert
            Assert.IsTrue(results.Count() == 0);
        }
    }
}
