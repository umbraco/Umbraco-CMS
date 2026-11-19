using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class ItemCountValidationHelperTests
{
    [TestCase(0, null)]
    [TestCase(1, null)]
    [TestCase(0, 0)]
    [TestCase(1, 0)]
    public void Is_False_When_No_Minimum_Is_Configured(int count, int? minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsAboveZeroAndBelowMinimum(count, minimum));

    [TestCase(1)]
    [TestCase(3)]
    public void Is_False_When_Count_Is_Zero(int minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsAboveZeroAndBelowMinimum(0, minimum));

    [TestCase(1, 3)]
    [TestCase(2, 3)]
    [TestCase(1, 2)]
    public void Is_True_When_Count_Is_Above_Zero_But_Below_Minimum(int count, int? minimum)
        => Assert.IsTrue(ItemCountValidationHelper.IsAboveZeroAndBelowMinimum(count, minimum));

    [TestCase(3, 3)]
    [TestCase(4, 3)]
    public void Is_False_When_Minimum_Is_Met_Or_Exceeded(int count, int? minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsAboveZeroAndBelowMinimum(count, minimum));
}
