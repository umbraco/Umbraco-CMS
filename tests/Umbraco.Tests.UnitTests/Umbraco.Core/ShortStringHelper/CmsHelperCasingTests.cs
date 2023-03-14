// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class CmsHelperCasingTests
{
    private IShortStringHelper ShortStringHelper =>
        new DefaultShortStringHelper(Options.Create(new RequestHandlerSettings()));

    [TestCase("thisIsTheEnd", "This Is The End")]
    [TestCase("th", "Th")]
    [TestCase("t", "t")]
    [TestCase("thisis", "Thisis")]
    [TestCase("ThisIsTheEnd", "This Is The End")]
    //// [TestCase("WhoIsNumber6InTheVillage", "Who Is Number6In The Village")] // note the issue with Number6In
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
