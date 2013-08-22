using System;
using System.IO;
using System.Web.Caching;
using NUnit.Framework;
using Umbraco.Core;
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
            ApplicationContext.Current = new ApplicationContext(true);
            ProfilerResolver.Current = new ProfilerResolver(new LogProfiler())
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

    }
}
