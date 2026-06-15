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

        Assert.That(key1, Is.EqualTo(key2));
        Assert.That(key1, Is.EqualTo(key2));
        Assert.That(key2.GetHashCode(), Is.EqualTo(key1.GetHashCode()));
    }

    [Test]
    public void Different_Keys_Are_Not_Equal()
    {
        var key1 = new CompositeStringArrayKey("a", "b", "c");
        var key2 = new CompositeStringArrayKey("a", "b", "d");

        Assert.That(key1, Is.Not.EqualTo(key2));
        Assert.That(key1, Is.Not.EqualTo(key2));
    }

    [Test]
    public void Keys_Are_Case_Insensitive()
    {
        var key1 = new CompositeStringArrayKey("Hello", "World");
        var key2 = new CompositeStringArrayKey("hello", "world");

        Assert.That(key1, Is.EqualTo(key2));
        Assert.That(key2.GetHashCode(), Is.EqualTo(key1.GetHashCode()));
    }

    [Test]
    public void Different_Length_Keys_Are_Not_Equal()
    {
        var key1 = new CompositeStringArrayKey("a", "b");
        var key2 = new CompositeStringArrayKey("a", "b", "c");

        Assert.That(key1, Is.Not.EqualTo(key2));
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
        Assert.That(dict.ContainsKey(lookup), Is.True);
        Assert.That(dict[lookup], Is.EqualTo("value"));
    }
}
