using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core.Collections;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Collections
{
    [TestFixture]
    public class DeepCloneableListTests
    {
        [Test]
        public void Deep_Clones_Each_Item_Once()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.CloneOnce);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(list.SequenceEqual(cloned));

            //Test that each instance in the list is not the same one
            foreach (var item in list)
            {
                var clone = cloned.Single(x => x.Id == item.Id);
                Assert.AreNotSame(item, clone);
            }

            //clone again from the clone - since it's clone once the items should be the same
            var cloned2 = cloned.DeepClone() as DeepCloneableList<TestClone>;

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(cloned.SequenceEqual(cloned2));

            //Test that each instance in the list is the same one
            foreach (var item in cloned)
            {
                var clone = cloned2.Single(x => x.Id == item.Id);
                Assert.AreSame(item, clone);
            }
        }

        [Test]
        public void Deep_Clones_All_Elements()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

            Assert.IsNotNull(cloned);
            Assert.AreNotSame(list, cloned);
            Assert.AreEqual(list.Count, cloned.Count);
        }

        [Test]
        public void Clones_Each_Item()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = (DeepCloneableList<TestClone>) list.DeepClone();

            foreach (var item in cloned)
            {
                Assert.IsTrue(item.IsClone);
            }
        }

        [Test]
        public void Cloned_Sequence_Equals()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = (DeepCloneableList<TestClone>)list.DeepClone();

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(list.SequenceEqual(cloned));

            //Test that each instance in the list is not the same one
            foreach (var item in list)
            {
                var clone = cloned.Single(x => x.Id == item.Id);
                Assert.AreNotSame(item, clone);
            }
        }

        public class TestClone : IDeepCloneable, IEquatable<TestClone>
        {
            public TestClone(Guid id)
            {
                Id = id;
                IsClone = true;
            }

            public TestClone()
            {
                Id = Guid.NewGuid();
            }

            public Guid Id { get; private set; }
            public bool IsClone { get; private set; }

            public object DeepClone()
            {
                return new TestClone(Id);
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(TestClone other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            /// <summary>
            /// Determines whether the specified object is equal to the current object.
            /// </summary>
            /// <returns>
            /// true if the specified object  is equal to the current object; otherwise, false.
            /// </returns>
            /// <param name="obj">The object to compare with the current object. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestClone)obj);
            }

            /// <summary>
            /// Serves as the default hash function. 
            /// </summary>
            /// <returns>
            /// A hash code for the current object.
            /// </returns>
            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(TestClone left, TestClone right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(TestClone left, TestClone right)
            {
                return Equals(left, right) == false;
            }
        }
    }
}
