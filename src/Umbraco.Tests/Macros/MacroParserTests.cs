using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Macros;

namespace Umbraco.Tests.Macros
{
    [TestFixture]
    public class MacroParserTests
    {
        [Test]
        public void Format_RTE_Data_For_Editor_With_No_Macros()
        {
            var content = @"<p>hello world</p>";
            var result = MacroTagParser.FormatRichTextContentForPersistence(content);
            Assert.AreEqual(@"<p>hello world</p>", content);
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Non_AlphaNumeric_Char_In_Alias()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""My.Map.isCool eh[boy!]"" />
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""My.Map.isCool eh[boy!]"" /> -->
<ins>Macro alias: <strong>My.Map.isCool eh[boy!]</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Editor()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" />
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>(){{"test1", "value1"},{"test2", "value2"}});

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));

            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Editor_Closing_Tag()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" />
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag_And_Content()
        {
            var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ><img src='blah.jpg'/></?UMBRACO_MACRO>
<p>asdfasdf</p>";
            var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>() { { "test1", "value1" }, { "test2", "value2" } });

//            Assert.AreEqual(@"<p>asdfasdf</p>
//<p>asdfsadf</p>
//<div class=""umb-macro-holder Map mceNonEditable"" test1=""value1"" test2=""value2"">
//<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
//<ins>Macro alias: <strong>Map</strong></ins></div>
//<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
            Assert.AreEqual(@"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Persistence()
        {
//            var content = @"<html>
//<body>
//<h1>asdfasdf</h1>
//<div class='umb-macro-holder Map mceNonEditable' att1='asdf' att2='asdfasdfasdf' att3=""sdfsdfd"">
//<!-- <?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" /> -->
//asdfasdf 
//asdfas
//<span>asdfasdfasdf</span>
//<p>asdfasdf</p>
//</div>
//<span>asdfdasf</span>
//<div>
//asdfsdf
//</div>
//</body>
//</html>";
            var content = @"<html>
<body>
<h1>asdfasdf</h1>
<div class='umb-macro-holder mceNonEditable' att1='asdf' att2='asdfasdfasdf' att3=""sdfsdfd"">
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
</html>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
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
</html>".Replace(Environment.NewLine, string.Empty), result.Replace(Environment.NewLine, string.Empty));
        }

        [Test]
        public void Format_RTE_Data_For_Persistence_Custom_Single_Entry()
        {
//            var content = @"<div class=""umb-macro-holder Test mceNonEditable umb-macro-mce_1""><!-- <?UMBRACO_MACRO macroAlias=""Test"" content=""1089"" textArea=""asdfasdf"" title="""" bool=""0"" number="""" contentType="""" multiContentType="""" multiProperties="""" properties="""" tabs="""" multiTabs="""" /> --><ins>
//<div class=""facts-box"">
//<div class=""fatcs-box-header"">
//<h3>null</h3>
//</div>
//<div class=""fatcs-box-body"">1089</div>
//</div>
//</ins></div>";
            var content = @"<div class=""umb-macro-holder mceNonEditable umb-macro-mce_1""><!-- <?UMBRACO_MACRO macroAlias=""Test"" content=""1089"" textArea=""asdfasdf"" title="""" bool=""0"" number="""" contentType="""" multiContentType="""" multiProperties="""" properties="""" tabs="""" multiTabs="""" /> --><ins>
<div class=""facts-box"">
<div class=""fatcs-box-header"">
<h3>null</h3>
</div>
<div class=""fatcs-box-body"">1089</div>
</div>
</ins></div>";
            var result = MacroTagParser.FormatRichTextContentForPersistence(content);

            Assert.AreEqual(@"<?UMBRACO_MACRO macroAlias=""Test"" content=""1089"" textArea=""asdfasdf"" title="""" bool=""0"" number="""" contentType="""" multiContentType="""" multiProperties="""" properties="""" tabs="""" multiTabs="""" />", result);
        }
    
    }
}