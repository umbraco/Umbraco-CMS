// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class CompositeStringArrayKeyTests
{
    [Test]
    public void Equal_Keys_Are_Equal()
    {
        var key1 = new CompositeStringArrayKey("a", "b", "c");
        var key2 = new CompositeStringArrayKey("a", "b", "c");

        Assert.IsTrue(key1.Equals(key2));
        Assert.IsTrue(key1 == key2);
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Different_Keys_Are_Not_Equal()
    {
        var key1 = new CompositeStringArrayKey("a", "b", "c");
        var key2 = new CompositeStringArrayKey("a", "b", "d");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Keys_Are_Case_Insensitive()
    {
        var key1 = new CompositeStringArrayKey("Hello", "World");
        var key2 = new CompositeStringArrayKey("hello", "world");

        Assert.IsTrue(key1.Equals(key2));
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Different_Length_Keys_Are_Not_Equal()
    {
        var key1 = new CompositeStringArrayKey("a", "b");
        var key2 = new CompositeStringArrayKey("a", "b", "c");

        Assert.IsFalse(key1.Equals(key2));
    }

    [Test]
    public void Null_Part_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeStringArrayKey("a", null!));
    }

    [Test]
    public void Works_As_Dictionary_Key()
    {
        var dict = new Dictionary<CompositeStringArrayKey, string>();
        var key = new CompositeStringArrayKey("culture", "segment", "fallback");

        dict[key] = "value";

        var lookup = new CompositeStringArrayKey("CULTURE", "SEGMENT", "FALLBACK");
        Assert.IsTrue(dict.ContainsKey(lookup));
        Assert.AreEqual("value", dict[lookup]);
    }
}
