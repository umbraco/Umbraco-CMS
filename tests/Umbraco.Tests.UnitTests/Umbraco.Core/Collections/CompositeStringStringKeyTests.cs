// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class CompositeStringStringKeyTests
{
    [Test]
    public void Equal_Keys_Are_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("a", "b");

        Assert.IsTrue(key1.Equals(key2));
        Assert.IsTrue(key1 == key2);
        Assert.IsFalse(key1 != key2);
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Different_FirstKeys_Are_Not_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("c", "b");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Different_SecondKeys_Are_Not_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("a", "c");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Swapped_Key_Parts_Are_Not_Equal()
    {
        var key1 = new CompositeStringStringKey("a", "b");
        var key2 = new CompositeStringStringKey("b", "a");

        Assert.IsFalse(key1.Equals(key2));
        Assert.IsTrue(key1 != key2);
    }

    [Test]
    public void Keys_Are_Case_Insensitive()
    {
        var key1 = new CompositeStringStringKey("Hello", "World");
        var key2 = new CompositeStringStringKey("hello", "world");

        Assert.IsTrue(key1.Equals(key2));
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Keys_Are_Compared_Ordinally()
    {
        // Ordinal case folding, not invariant-culture lowercasing: KELVIN SIGN and LATIN CAPITAL LETTER
        // SHARP S lowercase to "k" and to LATIN SMALL LETTER SHARP S under the invariant culture, but are
        // distinct code points ordinally.
        var key1 = new CompositeStringStringKey(CharAsString(0x212A), CharAsString(0x1E9E));
        var key2 = new CompositeStringStringKey("k", CharAsString(0x00DF));

        Assert.IsFalse(key1.Equals(key2));
    }

    [Test]
    public void Empty_Keys_Are_Equal()
    {
        var key1 = new CompositeStringStringKey(string.Empty, string.Empty);
        var key2 = new CompositeStringStringKey(string.Empty, string.Empty);

        Assert.IsTrue(key1.Equals(key2));
        Assert.AreEqual(key1.GetHashCode(), key2.GetHashCode());
    }

    [Test]
    public void Equals_Object_Compares_By_Value()
    {
        var key = new CompositeStringStringKey("a", "b");

        Assert.IsTrue(key.Equals((object)new CompositeStringStringKey("A", "B")));
        Assert.IsFalse(key.Equals((object)new CompositeStringStringKey("a", "c")));
        Assert.IsFalse(key.Equals(null));
        Assert.IsFalse(key.Equals("a"));
    }

    [Test]
    public void Works_As_Dictionary_Key()
    {
        var dict = new Dictionary<CompositeStringStringKey, string>();
        var key = new CompositeStringStringKey("en-US", "default");

        dict[key] = "value";

        var lookup = new CompositeStringStringKey("EN-us", "DEFAULT");
        Assert.IsTrue(dict.ContainsKey(lookup));
        Assert.AreEqual("value", dict[lookup]);
    }

    [Test]
    public void Default_Value_Is_Equal_To_Itself()
    {
        // The constructor rejects null parts, but default(T) bypasses it, leaving both parts null.
        Assert.IsTrue(default(CompositeStringStringKey).Equals(default(CompositeStringStringKey)));
        Assert.IsTrue(default(CompositeStringStringKey) == default);
    }

    [Test]
    public void Null_FirstKey_Throws()
    {
        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => new CompositeStringStringKey(null!, "b"));

        Assert.AreEqual("key1", exception?.ParamName);
    }

    [Test]
    public void Null_SecondKey_Throws()
    {
        ArgumentNullException? exception =
            Assert.Throws<ArgumentNullException>(() => new CompositeStringStringKey("a", null!));

        Assert.AreEqual("key2", exception?.ParamName);
    }

    private static string CharAsString(int codePoint) => ((char)codePoint).ToString();
}
