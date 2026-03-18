// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

/// <summary>
/// Tests for the <see cref="OrderedHashSet{T}"/> collection.
/// </summary>
[TestFixture]
public class OrderedHashSetTests
{
    /// <summary>
    /// Tests that the OrderedHashSet keeps the last added item when duplicates are added.
    /// </summary>
    [Test]
    public void Keeps_Last()
    {
        var list = new OrderedHashSet<MyClass>(false);
        MyClass[] items = { new MyClass("test"), new MyClass("test"), new MyClass("test") };
        foreach (var item in items)
        {
            list.Add(item);
        }

        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(items[2].Id, list[0].Id);
        Assert.AreNotEqual(items[0].Id, list[0].Id);
    }

    /// <summary>
    /// Tests that the OrderedHashSet keeps the first added item and ignores duplicates.
    /// </summary>
    [Test]
    public void Keeps_First()
    {
        var list = new OrderedHashSet<MyClass>();
        MyClass[] items = { new MyClass("test"), new MyClass("test"), new MyClass("test") };
        foreach (var item in items)
        {
            list.Add(item);
        }

        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(items[0].Id, list[0].Id);
    }

    private class MyClass : IEquatable<MyClass>
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="MyClass"/> class.
    /// </summary>
    /// <param name="name">The name to assign to the instance.</param>
        public MyClass(string name)
        {
            Name = name;
            Id = Guid.NewGuid();
        }

    /// <summary>
    /// Gets the name of this instance.
    /// </summary>
        public string Name { get; }

    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
        public Guid Id { get; }

    /// <summary>
    /// Determines whether the specified <see cref="MyClass"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="MyClass"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="MyClass"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
        public bool Equals(MyClass other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name);
        }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((MyClass)obj);
        }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>The hash code of the Name property.</returns>
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(MyClass left, MyClass right) => Equals(left, right);

        public static bool operator !=(MyClass left, MyClass right) => Equals(left, right) == false;
    }
}
