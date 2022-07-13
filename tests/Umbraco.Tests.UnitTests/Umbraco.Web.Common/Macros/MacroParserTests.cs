// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Macros;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Cms.Tests.Common.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Macros;

[TestFixture]
public class MacroParserTests
{
    [Test]
    [TestCase(@"<p>hello world</p>")]
    public void Format_RTE_Data_For_Editor_With_No_Macros(string expected)
    {
        var content = expected;
        var result = MacroTagParser.FormatRichTextContentForPersistence(content);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Non_AlphaNumeric_Char_In_Alias()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""My.Map.isCool eh[boy!]"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""My.Map.isCool eh[boy!]"" /> -->
<ins>Macro alias: <strong>My.Map.isCool eh[boy!]</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_Closing_Tag()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params_When_MacroAlias_Not_First()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO test1=""value1"" test2=""value2"" macroAlias=""Map"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO test1=""value1"" test2=""value2"" macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params_When_MacroAlias_Is_First()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params_When_Multiple_Macros()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO test1=""value1"" test2=""value2"" macroAlias=""Map"" />
<p>asdfsadf</p>
<?UMBRACO_MACRO test1=""value1"" macroAlias=""Map"" test2=""value2"" />
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO test1=""value1"" test2=""value2"" macroAlias=""Map"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO test1=""value1"" macroAlias=""Map"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Multiple_Macros()
    {
        var content = @"<p>asdfasdf</p>
<?UMBRACO_MACRO macroAlias=""Breadcrumb"" />
<p>asdfsadf</p>
<p> </p>
<?UMBRACO_MACRO macroAlias=""login"" />
<p> </p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(content, new Dictionary<string, string>());

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<div class=""umb-macro-holder mceNonEditable"">
<!-- <?UMBRACO_MACRO macroAlias=""Breadcrumb"" /> -->
<ins>Macro alias: <strong>Breadcrumb</strong></ins></div>
<p>asdfsadf</p>
<p> </p>
<div class=""umb-macro-holder mceNonEditable"">
<!-- <?UMBRACO_MACRO macroAlias=""login"" /> -->
<ins>Macro alias: <strong>login</strong></ins></div>
<p> </p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Persistence_Multiline_Parameters()
    {
        var content = @"<html>
<body>
<h1>asdfasdf</h1>
<div class='umb-macro-holder mceNonEditable' att1='asdf' att2='asdfasdfasdf' att3=""sdfsdfd"">
<!-- <?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2
dfdsfds"" /> -->
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

        Assert.AreEqual(
            @"<html>
<body>
<h1>asdfasdf</h1>
<?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2
dfdsfds"" />
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ></?UMBRACO_MACRO>
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Params_Closing_Tag_And_Content()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" ><img src='blah.jpg'/></?UMBRACO_MACRO>
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Editor_With_Multiline_Parameters()
    {
        var content = @"<p>asdfasdf</p>
<p>asdfsadf</p>
<?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2
test"" />
<p>asdfasdf</p>";
        var result = MacroTagParser.FormatRichTextPersistedDataForEditor(
            content,
            new Dictionary<string, string> { { "test1", "value1" }, { "test2", "value2\r\ntest" } });

        Assert.AreEqual(
            @"<p>asdfasdf</p>
<p>asdfsadf</p>
<div class=""umb-macro-holder mceNonEditable"" test1=""value1"" test2=""value2
test"">
<!-- <?UMBRACO_MACRO macroAlias=""Map"" test1=""value1"" test2=""value2
test"" /> -->
<ins>Macro alias: <strong>Map</strong></ins></div>
<p>asdfasdf</p>".NoCrLf(),
            result.NoCrLf());
    }

    [Test]
    public void Format_RTE_Data_For_Persistence()
    {
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

        Assert.AreEqual(
            @"<html>
<body>
<h1>asdfasdf</h1>
<?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" />
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>".StripNewLines(),
            result.StripNewLines());
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

        Assert.AreEqual(
            @"<html>
<body>
<h1>asdfasdf</h1>
<?UMBRACO_MACRO macroAlias=""myMacro"" param1=""test1"" param2=""test2"" />
<span>asdfdasf</span>
<div>
asdfsdf
</div>
</body>
</html>".StripNewLines(),
            result.StripNewLines());
    }

    [Test]
    public void Format_RTE_Data_For_Persistence_Custom_Single_Entry()
    {
        var content =
            @"<div class=""umb-macro-holder mceNonEditable umb-macro-mce_1""><!-- <?UMBRACO_MACRO macroAlias=""Test"" content=""1089"" textArea=""asdfasdf"" title="""" bool=""0"" number="""" contentType="""" multiContentType="""" multiProperties="""" properties="""" tabs="""" multiTabs="""" /> --><ins>
<div class=""facts-box"">
<div class=""fatcs-box-header"">
<h3>null</h3>
</div>
<div class=""fatcs-box-body"">1089</div>
</div>
</ins></div>";
        var result = MacroTagParser.FormatRichTextContentForPersistence(content);

        Assert.AreEqual(
            @"<?UMBRACO_MACRO macroAlias=""Test"" content=""1089"" textArea=""asdfasdf"" title="""" bool=""0"" number="""" contentType="""" multiContentType="""" multiProperties="""" properties="""" tabs="""" multiTabs="""" />",
            result);
    }
}
