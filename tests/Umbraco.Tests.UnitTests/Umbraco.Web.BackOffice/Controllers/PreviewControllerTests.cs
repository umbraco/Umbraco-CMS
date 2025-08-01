// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class PreviewControllerTests
{
    [TestCase("en-US", true)] // A framework culture.
    [TestCase("en-JP", true)] // A valid culture string, but not one that's in the framework.
    [TestCase("a!", false)]   // Not a valid culture string.
    [TestCase("<script>alert(123)</script>", false)]
    public void ValidateProvidedCulture_Validates_Culture(string culture, bool expectValid)
    {
        var result = PreviewController.ValidateProvidedCulture(culture);
        Assert.AreEqual(expectValid, result);
    }

    [Test]
    public void ValidateProvidedCulture_Validates_Culture_For_All_Framework_Cultures()
    {
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        foreach (var culture in cultures)
        {
            Assert.IsTrue(PreviewController.ValidateProvidedCulture(culture.Name), $"{culture.Name} is not considered a valid culture.");
            Assert.IsTrue(PreviewController.ValidateProvidedCulture(culture.Name.ToUpperInvariant()), $"{culture.Name.ToUpperInvariant()} is not considered a valid culture.");
            Assert.IsTrue(PreviewController.ValidateProvidedCulture(culture.Name.ToLowerInvariant()), $"{culture.Name.ToLowerInvariant()} is not considered a valid culture.");
        }
    }
}
