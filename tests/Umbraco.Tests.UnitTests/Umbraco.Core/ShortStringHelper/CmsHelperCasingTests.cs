// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

/// <summary>
/// Provides unit tests to verify the casing behavior of methods in the <see cref="CmsHelper"/> class.
/// </summary>
[TestFixture]
public class CmsHelperCasingTests
{
    private IShortStringHelper ShortStringHelper =>
        new DefaultShortStringHelper(Options.Create(new RequestHandlerSettings()));

    /// <summary>
    /// Tests that the SpaceCamelCasing extension method correctly converts camel case strings into space-separated, capitalized words.
    /// </summary>
    /// <param name="input">The camel case input string to convert.</param>
    /// <param name="expected">The expected output string with spaces and capitalization.</param>
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
