using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Strings;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Tests.CoreStrings
{
    [TestFixture]
    public class CmsHelperCasingTests
    {
        [TestCase("thisIsTheEnd", "This Is The End")]
        [TestCase("th", "Th")]
        [TestCase("t", "t")]
        [TestCase("thisis", "Thisis")]
        [TestCase("ThisIsTheEnd", "This Is The End")]
        [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6In The Village")] // note the issue with Number6In
        public void SpaceCamelCasing(string input, string expected)
        {
            var output = umbraco.cms.helpers.Casing.SpaceCamelCasing(input);
            Assert.AreEqual(expected, output);
        }

        [TestCase("thisIsTheEnd", "This Is The End")]
        [TestCase("th", "Th")]
        [TestCase("t", "t")]
        [TestCase("thisis", "Thisis")]
        [TestCase("ThisIsTheEnd", "This Is The End")]
        [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6In The Village")] // we're happy to reproduce the issue
        public void CompatibleLegacyReplacement(string input, string expected)
        {
            var helper = new LegacyShortStringHelper();
            var output = input.Length < 2 ? input : helper.SplitPascalCasing(input, ' ').ToFirstUpperInvariant();
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
            var helper = new DefaultShortStringHelper();
            var output = input.Length < 2 ? input : helper.SplitPascalCasing(input, ' ').ToFirstUpperInvariant();
            Assert.AreEqual(expected, output);
        }
    }
}
