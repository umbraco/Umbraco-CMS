using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Macros;

namespace Umbraco.Tests.Macros
{
    [TestFixture]
    public class MacroParserTests
    {
        [Test]
        public void Format_RTE_Data_For_Editor()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" />
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>(){{"test1", "value1"},{"test2", "value2"}});

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Editor_Closing_Tag()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" />
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag_And_Content()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ><img src='blah.jpg'/></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Persistence()
        {
            var content = @"<html>
<body>
<h1>asdfasdf</h1>
<div class='umb-macro-holder Map mceNonEditable' att1='asdf' att2='asdfasdfasdf' att3=""sdfsdfd"">
<!-- <?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" /> -->
asdfasdf 
asdfas
<span>asdfasdfasdf</span>
<p>asdfasdf</p>
</div>
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>";
            var result = MacroTagParser.FormatRichTextContentForPersistence(content);

            Assert.AreEqual(@"<html>
<body>
<h1>asdfasdf</h1>
<?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" />
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>", result);
        }

        [Test]
        public void Format_RTE_Data_For_Persistence_No_Class()
        {
            var content = @"<html>
<body>
<h1>asdfasdf</h1>
<div att1='asdf' att2='asdfasdfasdf' att3=""sdfsdfd"">
<!-- <?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" /> -->
asdfasdf 
asdfas
<span>asdfasdfasdf</span>
<p>asdfasdf</p>
</div>
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>";
            var result = MacroTagParser.FormatRichTextContentForPersistence(content);

            Assert.AreEqual(@"<html>
<body>
<h1>asdfasdf</h1>
<?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" />
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>", result);
        }
    }
}