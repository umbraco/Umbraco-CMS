using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="Range{T}"/> class in the <c>Umbraco.Core.Models</c> namespace.
/// </summary>
[TestFixture]
public class RangeTests
{
    [TestCase(0, 0, "0,0")]
    [TestCase(0, 1, "0,1")]
    [TestCase(1, 1, "1,1")]
    public void RangeInt32_ToString(int minimum, int maximum, string expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ToString());

    /// <summary>
    /// Verifies that the <see cref="Range{T}.ToString()"/> method returns the expected string representation for given minimum and maximum values.
    /// </summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    /// <param name="expected">The expected string representation of the range.</param>
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

    /// <summary>
    /// Verifies that the <see cref="Range{T}.ToString(string, string, IFormatProvider)"/> method returns the expected formatted string for given minimum and maximum values.
    /// </summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    /// <param name="expected">The expected string output from the ToString method.</param>
    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(0.5, 0.5, "0.5")]
    [TestCase(0.5, 1, "0.5,1")]
    public void RangeDouble_ToStringFormatRange(double minimum, double maximum, string expected) =>
        Assert.AreEqual(expected, new Range<double> { Minimum = minimum, Maximum = maximum }.ToString("{0}", "{0},{1}", CultureInfo.InvariantCulture));

    /// <summary>
    /// Tests that the <see cref="Range{T}.ToString(string, IFormatProvider)"/> method returns the expected formatted string for a given range and format string using invariant culture.
    /// </summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    /// <param name="expected">The expected string result after formatting the range with the format string "{0},{1}" and invariant culture.</param>
    [TestCase(0, 0, "0,0")]
    [TestCase(0, 1, "0,1")]
    [TestCase(1, 1, "1,1")]
    public void RangeInt32_ToStringFormat(int minimum, int maximum, string expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ToString("{0},{1}", CultureInfo.InvariantCulture));

    /// <summary>
    /// Tests the ToString method of the Range&lt;double&gt; struct with various minimum and maximum values and verifies the output format.
    /// </summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    /// <param name="expected">The expected string representation of the range.</param>
    [TestCase(0, 0.5, "0,0.5")]
    [TestCase(0.5, 0.5, "0.5,0.5")]
    [TestCase(0.5, 1, "0.5,1")]
    public void RangeDouble_ToStringFormat(double minimum, double maximum, string expected) =>
        Assert.AreEqual(expected, new Range<double> { Minimum = minimum, Maximum = maximum }.ToString("{0},{1}", CultureInfo.InvariantCulture));

    /// <summary>
    /// Tests the validity of a <see cref="Range{T}"/> instance with specified minimum and maximum values.
    /// </summary>
    /// <param name="minimum">The minimum value to assign to the range.</param>
    /// <param name="maximum">The maximum value to assign to the range.</param>
    /// <param name="expected">The expected result indicating whether the range is valid.</param>
    /// <remarks>A range is considered valid if the minimum is less than or equal to the maximum.</remarks>
    [TestCase(0, 0, true)]
    [TestCase(0, 1, true)]
    [TestCase(-1, 1, true)]
    [TestCase(1, 0, false)]
    [TestCase(0, -1, false)]
    public void RangeInt32_IsValid(int minimum, int maximum, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.IsValid());

    /// <summary>
    /// Unit test that verifies whether a given integer value is contained within a specified range defined by minimum and maximum values.
    /// </summary>
    /// <param name="minimum">The minimum value of the range.</param>
    /// <param name="maximum">The maximum value of the range.</param>
    /// <param name="value">The value to test for containment within the range.</param>
    /// <param name="expected">The expected result indicating whether the value should be considered within the range.</param>
    [TestCase(0, 0, 0, true)]
    [TestCase(0, 1, 0, true)]
    [TestCase(0, 1, 1, true)]
    [TestCase(-1, 1, 0, true)]
    [TestCase(0, 0, 1, false)]
    [TestCase(0, 0, -1, false)]
    public void RangeInt32_ContainsValue(int minimum, int maximum, int value, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum, Maximum = maximum }.ContainsValue(value));

    /// <summary>
    /// Tests whether the second integer range is entirely inside the first integer range.
    /// </summary>
    /// <param name="minimum1">The minimum value of the first (outer) range.</param>
    /// <param name="maximum1">The maximum value of the first (outer) range.</param>
    /// <param name="minimum2">The minimum value of the second (inner) range.</param>
    /// <param name="maximum2">The maximum value of the second (inner) range.</param>
    /// <param name="expected">True if the second range is entirely within the first range; otherwise, false.</param>
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

    /// <summary>
    /// Tests whether one range of integers contains another range.
    /// </summary>
    /// <param name="minimum1">The minimum value of the first range.</param>
    /// <param name="maximum1">The maximum value of the first range.</param>
    /// <param name="minimum2">The minimum value of the second range.</param>
    /// <param name="maximum2">The maximum value of the second range.</param>
    /// <param name="expected">The expected result indicating if the first range contains the second range.</param>
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

    /// <summary>
    /// Unit test that verifies the equality comparison between two <see cref="Range{T}"/> instances with specified minimum and maximum values.
    /// </summary>
    /// <param name="minimum1">The minimum value of the first range.</param>
    /// <param name="maximum1">The maximum value of the first range.</param>
    /// <param name="minimum2">The minimum value of the second range.</param>
    /// <param name="maximum2">The maximum value of the second range.</param>
    /// <param name="expected">True if the two ranges are expected to be equal; otherwise, false.</param>
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

    /// <summary>
    /// Tests the equality of two ranges defined by their minimum and maximum values.
    /// </summary>
    /// <param name="minimum1">The minimum value of the first range.</param>
    /// <param name="maximum1">The maximum value of the first range.</param>
    /// <param name="minimum2">The minimum value of the second range.</param>
    /// <param name="maximum2">The maximum value of the second range.</param>
    /// <param name="expected">The expected result of the equality comparison.</param>
    [TestCase(0, 0, 0, 0, true)]
    [TestCase(0, 1, 0, 1, true)]
    [TestCase(0, 1, 1, 1, false)]
    [TestCase(0, 0, 1, 1, false)]
    public void RangeInt32_EqualsValues(int minimum1, int maximum1, int minimum2, int maximum2, bool expected) =>
        Assert.AreEqual(expected, new Range<int> { Minimum = minimum1, Maximum = maximum1 }.Equals(minimum2, maximum2));
}
