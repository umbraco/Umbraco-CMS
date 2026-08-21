using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class IsoCodeValidatorTests
{
    private IsoCodeValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new IsoCodeValidator();

    [TestCase("en-US", ExpectedResult = true)]
    [TestCase("da-DK", ExpectedResult = true)]
    [TestCase("zh-CN", ExpectedResult = true)]
    [TestCase("en", ExpectedResult = true)]
    public bool Can_Validate_Standard_Culture(string isoCode)
        => _sut.IsValid(CultureInfo.GetCultureInfo(isoCode));

    [Test]
    public void Can_Validate_Every_Culture_Known_To_The_Platform()
    {
        CultureInfo[] known = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Where(culture => string.IsNullOrEmpty(culture.Name) is false)
            .ToArray();

        Assert.That(known.Where(culture => _sut.IsValid(culture) is false).Select(culture => culture.Name), Is.Empty);
    }

    [TestCase("en-NG", ExpectedResult = true)]
    [TestCase("ha-NG", ExpectedResult = true)]
    [TestCase("en-DK", ExpectedResult = true)]
    [TestCase("ar-PS", ExpectedResult = true)]
    [TestCase("zh-Hant-TW", ExpectedResult = true)]
    public bool Can_Validate_Culture_Without_Legacy_Windows_Lcid(string isoCode)
        => _sut.IsValid(CultureInfo.GetCultureInfo(isoCode));

    [TestCase("ca-ES-valencia", ExpectedResult = true)]
    [TestCase("zh-CHS", ExpectedResult = true)]
    public bool Can_Validate_Culture_Known_Only_By_Legacy_Windows_Lcid(string isoCode)
        => _sut.IsValid(CultureInfo.GetCultureInfo(isoCode));

    [Test]
    public void Cannot_Validate_Invariant_Culture()
        => Assert.That(_sut.IsValid(CultureInfo.InvariantCulture), Is.False);

    [TestCase("xx-XX")]
    [TestCase("qq-ZZ")]
    [TestCase("en-XY")]
    [TestCase("aa-BB")]
    [TestCase("zz")]
    public void Cannot_Validate_Culture_Unknown_To_The_Platform(string isoCode)
        => Assert.That(_sut.IsValid(CultureInfo.GetCultureInfo(isoCode)), Is.False);

    [TestCase("en-US")]
    [TestCase("en-NG")]
    public void Can_Validate_Standard_Culture_Via_String_Overload(string isoCode)
    {
        IIsoCodeValidator validator = _sut;
        Assert.That(validator.IsValid(isoCode), Is.True);
    }

    [TestCase("not-a-culture")]
    [TestCase("")]
    [TestCase("xx-XX")]
    [TestCase("en_US")]
    [TestCase("en-Latn-US")]
    public void Cannot_Validate_Invalid_IsoCode_Via_String_Overload(string isoCode)
    {
        IIsoCodeValidator validator = _sut;
        Assert.That(validator.IsValid(isoCode), Is.False);
    }
}
