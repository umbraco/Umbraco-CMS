// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Trees;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class EnumExtensionsTests
{
    [TestCase(TreeUse.Dialog, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Main, false)]
    [TestCase(TreeUse.Dialog | TreeUse.Main, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Dialog | TreeUse.Main, false)]
    public void HasFlagTest(TreeUse value, TreeUse test, bool expected)
    {
        // The built-in Enum.HasFlag() method determines whether
        // all bits from <test> are set (other bits can be set too).
        if (expected)
        {
            Assert.IsTrue(value.HasFlag(test));
        }
        else
        {
            Assert.IsFalse(value.HasFlag(test));
        }
    }

    [Obsolete]
    [TestCase(TreeUse.Dialog, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Main, false)]
    [TestCase(TreeUse.Dialog | TreeUse.Main, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Dialog | TreeUse.Main, false)]
    public void HasFlagAllTest(TreeUse value, TreeUse test, bool expected)
    {
        // The HasFlagAll() extension method determines whether
        // all bits from <test> are set (other bits can be set too).
        if (expected)
        {
            Assert.IsTrue(value.HasFlagAll(test));
        }
        else
        {
            Assert.IsFalse(value.HasFlagAll(test));
        }
    }

    [TestCase(TreeUse.Dialog, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Main, false)]
    [TestCase(TreeUse.Dialog | TreeUse.Main, TreeUse.Dialog, true)]
    [TestCase(TreeUse.Dialog, TreeUse.Dialog | TreeUse.Main, true)]
    public void HasFlagAnyTest(TreeUse value, TreeUse test, bool expected)
    {
        // The HasFlagAny() extension method determines whether
        // at least one bit from <test> is set.
        if (expected)
        {
            Assert.IsTrue(value.HasFlagAny(test));
        }
        else
        {
            Assert.IsFalse(value.HasFlagAny(test));
        }
    }
}
