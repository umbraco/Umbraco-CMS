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
        Assert.That(types.Length, Is.EqualTo(3));
        Assert.That(types, Does.Contain(typeof(Class2)));
        Assert.That(types, Does.Contain(typeof(Class1)));
        Assert.That(types, Does.Contain(typeof(object)));

        types = type.GetBaseTypes(false).ToArray();
        Assert.That(types.Length, Is.EqualTo(2));
        Assert.That(types, Does.Contain(typeof(Class1)));
        Assert.That(types, Does.Contain(typeof(object)));
    }

    private class Class1
    {
    }

    private class Class2 : Class1
    {
    }
}
