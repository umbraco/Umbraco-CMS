using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class RangeTests
{
    [TestCase(0, 0, "0,0")]
    [TestCase(0, 1, "0,1")]
    [TestCase(1, 1, "1,1")]
    public void RangeInt32_ToString(int minimum, int maximum, string expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ToString());

    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(0.5, 0.5, "0.5,0.5")]
    [TestCase(0.5, 1, "0.5,1")]
    public void RangeDouble_ToString(double minimum, double maximum, string expected) =>
        Assert.AreEqual(expected, new Range<double> { Minimum = minimum, Maximum = maximum }.ToString());

    [TestCase(0, 0, "0")]
    [TestCase(0, 1, "0,1")]
    [TestCase(1, 1, "1")]
    public void RangeInt32_ToStringFormatRange(int minimum, int maximum, string expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ToString("{0}", "{0},{1}", CultureInfo.InvariantCulture));

    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(0.5, 0.5, "0.5")]
    [TestCase(0.5, 1, "0.5,1")]
    public void RangeDouble_ToStringFormatRange(double minimum, double maximum, string expected) =>
        Assert.AreEqual(expected, new Range<double> { Minimum = minimum, Maximum = maximum }.ToString("{0}", "{0},{1}", CultureInfo.InvariantCulture));

    [TestCase(0, 0, "0,0")]
    [TestCase(0, 1, "0,1")]
    [TestCase(1, 1, "1,1")]
    public void RangeInt32_ToStringFormat(int minimum, int maximum, string expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ToString("{0},{1}", CultureInfo.InvariantCulture));

    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(0.5, 0.5, "0.5,0.5")]
    [TestCase(0.5, 1, "0.5,1")]
    public void RangeDouble_ToStringFormat(double minimum, double maximum, string expected) =>
        Assert.AreEqual(expected, new Range<double> { Minimum = minimum, Maximum = maximum }.ToString("{0},{1}", CultureInfo.InvariantCulture));

    [TestCase(0, 0, true)]
    [TestCase(0, 1, true)]
    [TestCase(-1, 1, true)]
    [TestCase(1, 0, false)]
    [TestCase(0, -1, false)]
    public void RangeInt32_IsValid(int minimum, int maximum, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.IsValid());

    [TestCase(0, 0, 0, true)]
    [TestCase(0, 1, 0, true)]
    [TestCase(0, 1, 1, true)]
    [TestCase(-1, 1, 0, true)]
    [TestCase(0, 0, 1, false)]
    [TestCase(0, 0, -1, false)]
    public void RangeInt32_ContainsValue(int minimum, int maximum, int value, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ContainsValue(value));

    [TestCase(0, 0, 0, 0, true)]
    [TestCase(0, 1, 0, 1, true)]
    [TestCase(0, 1, 1, 1, false)]
    [TestCase(-1, 1, 0, 0, false)]
    [TestCase(0, 0, 1, 1, false)]
    [TestCase(0, 0, -1, 1, true)]
    public void RangeInt32_IsInsideRange(int minimum1, int maximum1, int minimum2, int maximum2, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum1, Maximum = maximum1 }.IsInsideRange(new Range<int>
            {
                Minimum = minimum2,
                Maximum = maximum2,
            }));

    [TestCase(0, 0, 0, 0, true)]
    [TestCase(0, 1, 0, 1, true)]
    [TestCase(0, 1, 1, 1, true)]
    [TestCase(-1, 1, 0, 0, true)]
    [TestCase(0, 0, 1, 1, false)]
    [TestCase(0, 0, -1, 1, false)]
    public void RangeInt32_ContainsRange(int minimum1, int maximum1, int minimum2, int maximum2, bool expected) =>
        Assert.AreEqual(
            expected,
            new Range<int> { Minimum = minimum1, Maximum = maximum1 }.ContainsRange(new Range<int>
            {
                Minimum = minimum2,
                Maximum = maximum2,
            }));

    [TestCase(0, 0, 0, 0, true)]
    [TestCase(0, 1, 0, 1, true)]
    [TestCase(0, 1, 1, 1, false)]
    [TestCase(0, 0, 1, 1, false)]
    public void RangeInt32_Equals(int minimum1, int maximum1, int minimum2, int maximum2, bool expected) =>
        Assert.AreEqual(
            expected,
            new Range<int> { Minimum = minimum1, Maximum = maximum1 }.Equals(new Range<int>
            {
                Minimum = minimum2,
                Maximum = maximum2,
            }));

    [TestCase(0, 0, 0, 0, true)]
    [TestCase(0, 1, 0, 1, true)]
    [TestCase(0, 1, 1, 1, false)]
    [TestCase(0, 0, 1, 1, false)]
    public void RangeInt32_EqualsValues(int minimum1, int maximum1, int minimum2, int maximum2, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum1, Maximum = maximum1 }.Equals(minimum2, maximum2));
}
