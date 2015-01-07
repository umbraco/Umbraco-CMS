using System;
using System.IO;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Profiling;
using umbraco;
using umbraco.cms.businesslogic.macro;

namespace Umbraco.Tests.Macros
{
    [TestFixture]
    public class MacroTests
    {

        [SetUp]
        public void Setup()
        {
            //we DO want cache enabled for these tests
            var cacheHelper = new CacheHelper(
                new ObjectCacheRuntimeCacheProvider(),
                new StaticCacheProvider(),
                new NullCacheProvider());
            ApplicationContext.Current = new ApplicationContext(cacheHelper);
            ProfilerResolver.Current = new ProfilerResolver(new LogProfiler(Mock.Of<ILogger>()))
            {
                CanResolveBeforeFrozen = true
            };
        }

        [TearDown]
        public void TearDown()
        {
            ProfilerResolver.Current.DisposeIfDisposable();
            ProfilerResolver.Reset();
            ApplicationContext.Current.ApplicationCache.ClearAllCache();
            ApplicationContext.Current.DisposeIfDisposable();
            ApplicationContext.Current = null;
        }

        [TestCase("123", "IntProp", typeof(int))]
        [TestCase("Hello", "StringProp", typeof(string))]
        [TestCase("123456789.01", "DoubleProp", typeof(double))]
        [TestCase("1234567", "FloatProp", typeof(float))]
        [TestCase("1", "BoolProp", typeof(bool))]
        [TestCase("true", "BoolProp", typeof(bool))]
        [TestCase("0", "BoolProp", typeof(bool))]
        [TestCase("false", "BoolProp", typeof(bool))]
        [TestCase("2001-05-10", "DateProp", typeof(DateTime))]
        [TestCase("123px", "UnitProp", typeof(Unit))]
        [TestCase("456pt", "UnitProp", typeof(Unit))]
        [TestCase("CEC063D3-D73E-4B7D-93ED-7F69CA9BF2D0", "GuidProp", typeof(Guid))]
        [TestCase("CEC063D3D73E4B7D93ED7F69CA9BF2D0", "GuidProp", typeof(Guid))]
        [TestCase("", "NullDateProp", typeof(DateTime?))]
        [TestCase("2001-05-10", "NullDateProp", typeof(DateTime?))]
        [TestCase("", "NullUnitProp", typeof(Unit?))]
        [TestCase("456pt", "NullUnitProp", typeof(Unit?))]
        public void SetUserControlProperty(string val, string macroPropName, Type convertTo)
        {
            var ctrl = new UserControlTest();
            var macroModel = new MacroModel("test", "test", "", "~/usercontrols/menu.ascx", "", "", 0, false, false);
            macroModel.Properties.Add(new MacroPropertyModel(macroPropName, val));

            macro.UpdateControlProperties(ctrl, macroModel);

            var ctrlType = ctrl.GetType();
            var prop = ctrlType.GetProperty(macroPropName);
            var converted = val.TryConvertTo(convertTo);

            Assert.IsTrue(converted.Success);
            Assert.NotNull(prop);
            Assert.AreEqual(converted.Result, prop.GetValue(ctrl));
        }

        [TestCase("text.xslt", "", "", "", "XSLT")]
        [TestCase("", "razor-script.cshtml", "", "", "Script")]
        [TestCase("", "~/Views/MacroPartials/test.cshtml", "", "", "PartialView")]
        [TestCase("", "~/App_Plugins/MyPackage/Views/MacroPartials/test.cshtml", "", "", "PartialView")]
        [TestCase("", "", "~/usercontrols/menu.ascx", "", "UserControl")]
        [TestCase("", "", "~/usercontrols/Header.ASCX", "", "UserControl")]
        [TestCase("", "", "MyNamespace.MyCustomControl", "MyAssembly", "CustomControl")]
        [TestCase("", "", "", "", "Unknown")]
        public void Determine_Macro_Type(string xslt, string scriptFile, string scriptType, string scriptAssembly, string expectedType)
        {
            var expected = Enum<MacroTypes>.Parse(expectedType);
            Assert.AreEqual(expected, Macro.FindMacroType(xslt, scriptFile, scriptType, scriptAssembly));
        }

        [TestCase("text.xslt", "", "", "", "~/xslt/text.xslt")]
        [TestCase("", "razor-script.cshtml", "", "", "~/macroScripts/razor-script.cshtml")]
        [TestCase("", "~/Views/MacroPartials/test.cshtml", "", "", "~/Views/MacroPartials/test.cshtml")]
        [TestCase("", "~/App_Plugins/MyPackage/Views/MacroPartials/test.cshtml", "", "", "~/App_Plugins/MyPackage/Views/MacroPartials/test.cshtml")]
        [TestCase("", "", "~/usercontrols/menu.ascx", "", "~/usercontrols/menu.ascx")]
        public void Get_Macro_File(string xslt, string scriptFile, string scriptType, string scriptAssembly, string expectedResult)
        {
            var model = new MacroModel("Test", "test", scriptAssembly, scriptType, xslt, scriptFile, 0, false, false);
            var file = macro.GetMacroFile(model);
            Assert.AreEqual(expectedResult, file);
        }

        [TestCase("XSLT", true)]
        [TestCase("Script", true)]
        [TestCase("PartialView", true)]
        [TestCase("UserControl", true)]
        [TestCase("CustomControl", false)]
        [TestCase("Python", true)]
        [TestCase("Unknown", false)]
        public void Macro_Is_File_Based(string macroType, bool expectedResult)
        {
            var mType = Enum<MacroTypes>.Parse(macroType);
            var model = new MacroModel("Test", "test", "", "", "", "", 0, false, false);
            model.MacroType = mType; //force the type
            Assert.AreEqual(expectedResult, macro.MacroIsFileBased(model));
        }

        [TestCase("XSLT", true)]
        [TestCase("Script", true)]
        [TestCase("PartialView", true)]
        [TestCase("UserControl", false)]
        [TestCase("CustomControl", false)]
        [TestCase("Python", true)]
        [TestCase("Unknown", false)]
        public void Can_Cache_As_String(string macroType, bool expectedResult)
        {
            var mType = Enum<MacroTypes>.Parse(macroType);
            var model = new MacroModel("Test", "test", "", "", "", "", 0, false, false);
            model.MacroType = mType; //force the type
            Assert.AreEqual(expectedResult, macro.CacheMacroAsString(model));
        }

        [TestCase(-5, true)] //the cache DateTime will be older than the file date
        [TestCase(5, false)] //the cache DateTime will be newer than the file date
        public void Macro_Needs_Removing_Based_On_Macro_File(int minutesToNow, bool expectedResult)
        {
            var now = DateTime.Now;
            ApplicationContext.Current.ApplicationCache.InsertCacheItem(
                "TestDate",
                CacheItemPriority.NotRemovable,
                new TimeSpan(0, 0, 60),
                () => now.AddMinutes(minutesToNow)); //add a datetime value of 'now' with the minutes offset

            //now we need to update a file's date to 'now' to compare
            var path = Path.Combine(TestHelpers.TestHelper.CurrentAssemblyDirectory, "temp.txt");
            File.CreateText(path).Close();

            //needs to be file based (i.e. xslt)
            var model = new MacroModel("Test", "test", "", "", "test.xslt", "", 0, false, false);

            Assert.AreEqual(expectedResult, macro.MacroNeedsToBeClearedFromCache(model, "TestDate", new FileInfo(path)));
        }

        public void Get_Macro_Cache_Identifier()
        {
            //var asdf  = macro.GetCacheIdentifier()
        }

        private class UserControlTest : UserControl
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
            public double DoubleProp { get; set; }
            public float FloatProp { get; set; }
            public bool BoolProp { get; set; }
            public DateTime DateProp { get; set; }
            public Unit UnitProp { get; set; }
            public Guid GuidProp { get; set; }
            public DateTime? NullDateProp { get; set; }
            public Unit? NullUnitProp { get; set; }
        }
    }
}
