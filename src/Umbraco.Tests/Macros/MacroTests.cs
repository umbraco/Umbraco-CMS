using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Macros;

namespace Umbraco.Tests.Macros
{
    [TestFixture]
    public class MacroTests
    {

        [SetUp]
        public void Setup()
        {
            //we DO want cache enabled for these tests
            var cacheHelper = new AppCaches(
                new ObjectCacheAppCache(),
                NoAppCache.Instance,
                new IsolatedCaches(type => new ObjectCacheAppCache()));
            //Current.ApplicationContext = new ApplicationContext(cacheHelper, new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>()));

            Current.Reset();
            Current.UnlockConfigs();
            Current.Configs.Add(SettingsForTests.GetDefaultUmbracoSettings);
        }

        [TestCase("PartialView", true)]
        [TestCase("Unknown", false)]
        public void Macro_Is_File_Based(string macroTypeString, bool expectedNonNull)
        {
            var macroType = Enum<MacroTypes>.Parse(macroTypeString);
            var model = new MacroModel
            {
                MacroType = macroType,
                MacroSource = "anything"
            };
            var filename = MacroRenderer.GetMacroFileName(model);
            if (expectedNonNull)
                Assert.IsNotNull(filename);
            else
                Assert.IsNull(filename);
        }

        //[TestCase(-5, true)] //the cache DateTime will be older than the file date
        //[TestCase(5, false)] //the cache DateTime will be newer than the file date
        public void Macro_Needs_Removing_Based_On_Macro_File(int minutesToNow, bool expectedNull)
        {
            // macro has been refactored, and macro.GetMacroContentFromCache() will
            // take care of the macro file, if any. It requires a web environment,
            // so we cannot really test this anymore.
        }

        public void Get_Macro_Cache_Identifier()
        {
            //var asdf  = macro.GetCacheIdentifier()
        }
    }
}
