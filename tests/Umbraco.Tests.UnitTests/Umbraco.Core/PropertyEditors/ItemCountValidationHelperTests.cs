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
    public void Is_Not_Below_Minimum_When_No_Minimum_Is_Configured(int count, int? minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsBelowMinimum(count, minimum));

    [TestCase(1)]
    [TestCase(3)]
    public void Is_Not_Below_Minimum_When_Collection_Is_Empty(int minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsBelowMinimum(0, minimum));

    [TestCase(1, 3)]
    [TestCase(2, 3)]
    [TestCase(1, 2)]
    public void Is_Below_Minimum_When_Collection_Is_In_Use_But_Short(int count, int? minimum)
        => Assert.IsTrue(ItemCountValidationHelper.IsBelowMinimum(count, minimum));

    [TestCase(3, 3)]
    [TestCase(4, 3)]
    public void Is_Not_Below_Minimum_When_Minimum_Is_Met_Or_Exceeded(int count, int? minimum)
        => Assert.IsFalse(ItemCountValidationHelper.IsBelowMinimum(count, minimum));
}
