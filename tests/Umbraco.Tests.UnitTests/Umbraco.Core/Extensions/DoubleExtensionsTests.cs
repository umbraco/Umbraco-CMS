using System.Globalization;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class DoubleExtensionsTests
{
    [TestCase(1.65, "1.65")]
    [TestCase(-1.65, "-1.65")]
    [TestCase(10.45, "10.45")]
    [TestCase(123.45, "123.45")]
    [TestCase(0.000001, "0.000001")]
    [TestCase(0.0000001, "0.0000001")]
    [TestCase(1.1234567, "1.1234567")]
    [TestCase(0, "0")]
    [TestCase(123456789, "123456789")]
    public void ToShortestDecimal_Uses_The_Shortest_Representation_Of_A_Double(double value, string expected)
        => Assert.AreEqual(decimal.Parse(expected, CultureInfo.InvariantCulture), value.ToShortestDecimal());

    [TestCase(1.65f, "1.65")]
    [TestCase(123.45f, "123.45")]
    [TestCase(-0.5f, "-0.5")]
    public void ToShortestDecimal_Uses_The_Shortest_Representation_Of_A_Float(float value, string expected)
        => Assert.AreEqual(decimal.Parse(expected, CultureInfo.InvariantCulture), value.ToShortestDecimal());

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(1e30)]
    public void ToShortestDecimal_Throws_For_Values_A_Decimal_Cannot_Hold(double value)
        => Assert.Throws<OverflowException>(() => value.ToShortestDecimal());
}
