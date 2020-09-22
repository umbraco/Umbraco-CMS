using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Strings;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Strings
{
    [TestFixture]
    public class CmsHelperCasingTests
    {
        private IShortStringHelper ShortStringHelper => new DefaultShortStringHelper(Options.Create(new RequestHandlerSettings()));

        [TestCase("thisIsTheEnd", "This Is The End")]
        [TestCase("th", "Th")]
        [TestCase("t", "t")]
        [TestCase("thisis", "Thisis")]
        [TestCase("ThisIsTheEnd", "This Is The End")]
        //[TestCase("WhoIsNumber6InTheVillage", "Who Is Number6In The Village")] // note the issue with Number6In
        [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6 In The Village")] // now fixed since DefaultShortStringHelper is the default
        public void SpaceCamelCasing(string input, string expected)
        {
            var output = input.SpaceCamelCasing(ShortStringHelper);
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

            var output = input.Length < 2 ? input : ShortStringHelper.SplitPascalCasing(input, ' ').ToFirstUpperInvariant();
            Assert.AreEqual(expected, output);
        }
    }
}
