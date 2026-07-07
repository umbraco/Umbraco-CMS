using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors;

[TestFixture]
public class ImageCropperCropEqualityTests
{
    [Test]
    public void Crops_Are_Equal_When_All_Properties_Match_Including_AltTextByCulture()
    {
        var left = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["da-DK"] = "Dansk", ["en-US"] = "English" });
        var right = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["en-US"] = "English", ["da-DK"] = "Dansk" });

        Assert.IsTrue(left.Equals(right));
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [Test]
    public void Crops_Are_Not_Equal_When_AltTextByCulture_Values_Differ()
    {
        var left = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["da-DK"] = "Dansk" });
        var right = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["da-DK"] = "Anden tekst" });

        Assert.IsFalse(left.Equals(right));
    }

    [Test]
    public void Crops_Are_Not_Equal_When_AltTextByCulture_Keys_Differ()
    {
        var left = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["da-DK"] = "Dansk" });
        var right = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["en-US"] = "Dansk" });

        Assert.IsFalse(left.Equals(right));
    }

    [Test]
    public void Crops_Are_Not_Equal_When_Only_One_Has_AltTextByCulture()
    {
        var left = CreateCrop(altTextByCulture: new Dictionary<string, string> { ["da-DK"] = "Dansk" });
        var right = CreateCrop(altTextByCulture: null);

        Assert.IsFalse(left.Equals(right));
        Assert.IsFalse(right.Equals(left));
    }

    [Test]
    public void Crops_Are_Equal_When_Both_Have_No_AltTextByCulture()
    {
        var left = CreateCrop(altTextByCulture: null);
        var right = CreateCrop(altTextByCulture: null);

        Assert.IsTrue(left.Equals(right));
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    private static ImageCropperValue.ImageCropperCrop CreateCrop(Dictionary<string, string>? altTextByCulture)
        => new()
        {
            Alias = "portrait",
            Width = 800,
            Height = 600,
            AltText = "Default alt text",
            Coordinates = new ImageCropperValue.ImageCropperCropCoordinates { X1 = 0.1m, Y1 = 0.1m, X2 = 0.9m, Y2 = 0.9m },
            AltTextByCulture = altTextByCulture,
        };
}
