using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Collections
{
    [TestFixture]
    public class OrderedHashSetTests
    {
        [Test]
        public void Keeps_Last()
        {
            var list = new OrderedHashSet<MyClass>(keepOldest:false);
            var items = new MyClass[] {new MyClass("test"), new MyClass("test"), new MyClass("test") };
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
            var list = new OrderedHashSet<MyClass>(keepOldest: true);
            var items = new MyClass[] { new MyClass("test"), new MyClass("test"), new MyClass("test") };
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

            public string Name { get; private set; }
            public Guid Id { get; private set; }

            public bool Equals(MyClass other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MyClass) obj);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public static bool operator ==(MyClass left, MyClass right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(MyClass left, MyClass right)
            {
                return !Equals(left, right);
            }
        }
    }
}