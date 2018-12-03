using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class CmsHelperCasingTests : UmbracoTestBase
    {
        [SetUp]
        public void Setup()
        {
            //set default config
            var config = SettingsForTests.GetDefaultUmbracoSettings();
            SettingsForTests.ConfigureSettings(config);

        }

        [TestCase("thisIsTheEnd", "This Is The End")]
        [TestCase("th", "Th")]
        [TestCase("t", "t")]
        [TestCase("thisis", "Thisis")]
        [TestCase("ThisIsTheEnd", "This Is The End")]
        //[TestCase("WhoIsNumber6InTheVillage", "Who Is Number6In The Village")] // note the issue with Number6In
        [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6 In The Village")] // now fixed since DefaultShortStringHelper is the default
        public void SpaceCamelCasing(string input, string expected)
        {
            var output = input.SpaceCamelCasing();
            Assert.AreEqual(expected, output);
        }

        [TestCase("thisIsTheEnd", "This Is The End")]
        [TestCase("th", "Th")]
        [TestCase("t", "t")]
        [TestCase("thisis", "Thisis")]
        [TestCase("ThisIsTheEnd", "This Is The End")]
        [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6 In The Village")] // issue is fixed
        public void CompatibleDefaultReplacement(string input, string expected)
        {
            var helper = new DefaultShortStringHelper(SettingsForTests.GetDefaultUmbracoSettings());
            var output = input.Length < 2 ? input : helper.SplitPascalCasing(input, ' ').ToFirstUpperInvariant();
            Assert.AreEqual(expected, output);
        }
    }
}
