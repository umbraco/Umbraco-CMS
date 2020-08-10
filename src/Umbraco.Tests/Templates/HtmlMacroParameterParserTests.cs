using Umbraco.Core.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web.Templates;
using Umbraco.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Routing;
using Umbraco.Tests.Testing.Objects;
using System.Web;
using System;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Umbraco.Tests.Templates
{
    [TestFixture]
    public class HtmlMacroParameterParserTests
    {
        [Test]
        public void Returns_Udis_From_Single_MediaPicker_Macro_Parameters_In_Macros_In_Html()
        {
            //Two macros, one single parameter, single Media Picker, one multiple paramters of single media picker
            var input = @"<p>This is some normal text before the macro</p>
                        <?UMBRACO_MACRO macroAlias=""ThreeSingleMediaPickers"" singleMediaPicker1=""umb://media/eee91c05b2e84031a056dcd7f28eff89"" singleMediaPicker2=""umb://media/fa763e0d0ceb408c8720365d57e06e32"" singleMediaPicker3="""" />
                        <p>This is a paragraph after the macro and before the next</p>
                        <?UMBRACO_MACRO macroAlias=""SingleMediaPicker"" singleMediaPicker=""umb://media/90ba0d3dba6e4c9fa1953db78352ba73"" />
                        <p>some more text</p>";

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromMacroParameters(input).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/eee91c05b2e84031a056dcd7f28eff89"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://media/fa763e0d0ceb408c8720365d57e06e32"), result[1]);
            Assert.AreEqual(Udi.Parse("umb://media/90ba0d3dba6e4c9fa1953db78352ba73"), result[2]);
        }
        [Test]
        public void Returns_Empty_With_From_Single_MediaPicker_With_No_Macro_Parameters_In_Macros_In_Html()
        {
            //Two macros, one single parameter, single Media Picker, one multiple paramters of single media picker
            var input = @"<p>This is some normal text before the macro</p>
                        <?UMBRACO_MACRO macroAlias=""ThreeSingleMediaPickers""  />
                        <p>This is a paragraph after the macro and before the next</p>
                        <?UMBRACO_MACRO macroAlias=""AnotherParameter"" singleMediaPicker=""<p>Some other value</p>"" />
                        <p>some more text</p>";

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromMacroParameters(input).ToList();
            Assert.AreEqual(0, result.Count);         
        }
        //NB: When multiple media pickers store udis instead of ints! - see https://github.com/umbraco/Umbraco-CMS/pull/8388 
        [Test]
        public void Returns_Empty_When_No_Macros_In_Html()
        {
            //Two macros, one single parameter, single Media Picker, one multiple paramters of single media picker
            var input = @"<p>This is some normal text before the macro</p>                       
                        <p>This is a paragraph after the macro and before the next</p>
                        <p>some more text</p>";

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromMacroParameters(input).ToList();
            Assert.AreEqual(0, result.Count);           
        }
        [Test]
        public void Returns_Udis_From_Multiple_MediaPicker_Macro_Parameters_In_Macros_In_Html()
        {
            //Two macros, one single parameter, single Media Picker, one multiple paramters of single media picker
            var input = @"<p>This is some normal text before the macro</p>
                        <?UMBRACO_MACRO macroAlias=""MultipleMediaPickers"" multipleMediaPicker1=""umb://media/eee91c05b2e84031a056dcd7f28eff89,umb://media/fa763e0d0ceb408c8720365d57e06e32,umb://media/bb763e0d0ceb408c8720365d57e06444""  />
                        <p>This is a paragraph after the macro</p>";

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromMacroParameters(input).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/eee91c05b2e84031a056dcd7f28eff89"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://media/fa763e0d0ceb408c8720365d57e06e32"), result[1]);
            Assert.AreEqual(Udi.Parse("umb://media/bb763e0d0ceb408c8720365d57e06444"), result[2]);
        }
        [Test]
        public void Returns_Udis_From_Single_MediaPicker_Macro_Parameters_In_Grid_Macros()
        {
            // create a list of GridValue.GridControls with Editor GridEditor alias macro
            List<GridValue.GridControl> gridControls = new List<GridValue.GridControl>();

            // single media picker macro parameter
            var macroGridControl = GetMacroGridControl(@"{  ""macroAlias"": ""SingleMediaPicker"",  ""macroParamsDictionary"": {    ""singleMediaPicker"": ""umb://media/90ba0d3dba6e4c9fa1953db78352ba73""  }}");
            gridControls.Add(macroGridControl);
            
            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromGridControlMacroParameters(gridControls).ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/90ba0d3dba6e4c9fa1953db78352ba73"), result[0]);
           
        }
        [Test]
        public void Returns_Empty_From_Single_MediaPicker_With_No_Macro_Parameters_In_Grid_Macros()
        {
            // create a list of GridValue.GridControls with Editor GridEditor alias macro
            List<GridValue.GridControl> gridControls = new List<GridValue.GridControl>();

            // single media picker macro parameter
            var macroGridControl = GetMacroGridControl(@"{  ""macroAlias"": ""SingleMediaPicker"",  ""macroParamsDictionary"": {}}");
            gridControls.Add(macroGridControl);

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromGridControlMacroParameters(gridControls).ToList();
            Assert.AreEqual(0, result.Count);           

        }
        //NB: When multiple media pickers store udis instead of ints! - see https://github.com/umbraco/Umbraco-CMS/pull/8388 
        [Test]
        public void Returns_Udis_From_Multiple_MediaPicker_Macro_Parameters_In_Grid_Macros()
        {
            // create a list of GridValue.GridControls with Editor GridEditor alias macro
            List<GridValue.GridControl> gridControls = new List<GridValue.GridControl>();

            // multiple media picker macro parameter
            var macroGridControl = GetMacroGridControl(@"{  ""macroAlias"": ""SingleMediaPicker"",  ""macroParamsDictionary"": {    ""multipleMediaPicker"": ""umb://media/eee91c05b2e84031a056dcd7f28eff89,umb://media/fa763e0d0ceb408c8720365d57e06e32,umb://media/bb763e0d0ceb408c8720365d57e06444""  }}");
            gridControls.Add(macroGridControl);

            var macroParameterParser = new HtmlMacroParameterParser();

            var result = macroParameterParser.FindUdisFromGridControlMacroParameters(gridControls).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(Udi.Parse("umb://media/eee91c05b2e84031a056dcd7f28eff89"), result[0]);
            Assert.AreEqual(Udi.Parse("umb://media/fa763e0d0ceb408c8720365d57e06e32"), result[1]);
            Assert.AreEqual(Udi.Parse("umb://media/bb763e0d0ceb408c8720365d57e06444"), result[2]);

        }

        //setup a Macro Grid Control based on Json of the Macro
        private GridValue.GridControl GetMacroGridControl(string macroJson)
        {
            var macroGridEditor = new GridValue.GridEditor();
            macroGridEditor.Alias = "macro";
            macroGridEditor.View = "macro";
            var macroGridControl = new GridValue.GridControl();
            macroGridControl.Editor = macroGridEditor;
            macroGridControl.Value = JToken.Parse(macroJson);
            return macroGridControl;
        }

    }
}
