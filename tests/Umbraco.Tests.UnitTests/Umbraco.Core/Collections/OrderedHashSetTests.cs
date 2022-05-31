// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class OrderedHashSetTests
{
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
        public MyClass(string name)
        {
            Name = name;
            Id = Guid.NewGuid();
        }

        public string Name { get; }

        public Guid Id { get; }

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

        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(MyClass left, MyClass right) => Equals(left, right);

        public static bool operator !=(MyClass left, MyClass right) => Equals(left, right) == false;
    }
}
