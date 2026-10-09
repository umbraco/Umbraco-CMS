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
    public void Cannot_Validate_Invariant_Culture()
        => Assert.That(_sut.IsValid(CultureInfo.InvariantCulture), Is.False);

    [Test]
    public void Can_Validate_Standard_Culture_Via_String_Overload()
    {
        IIsoCodeValidator validator = _sut;
        Assert.That(validator.IsValid("en-US"), Is.True);
    }

    [TestCase("not-a-culture")]
    [TestCase("")]
    public void Cannot_Validate_Invalid_IsoCode_Via_String_Overload(string isoCode)
    {
        IIsoCodeValidator validator = _sut;
        Assert.That(validator.IsValid(isoCode), Is.False);
    }
}
