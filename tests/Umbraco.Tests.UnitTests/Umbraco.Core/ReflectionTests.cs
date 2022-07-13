// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class ReflectionTests
{
    [Test]
    public void GetBaseTypesIsOk()
    {
        // tests that the GetBaseTypes extension method works.
        var type = typeof(Class2);
        var types = type.GetBaseTypes(true).ToArray();
        Assert.AreEqual(3, types.Length);
        Assert.Contains(typeof(Class2), types);
        Assert.Contains(typeof(Class1), types);
        Assert.Contains(typeof(object), types);

        types = type.GetBaseTypes(false).ToArray();
        Assert.AreEqual(2, types.Length);
        Assert.Contains(typeof(Class1), types);
        Assert.Contains(typeof(object), types);
    }

    private class Class1
    {
    }

    private class Class2 : Class1
    {
    }
}
