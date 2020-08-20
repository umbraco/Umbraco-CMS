using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Tests.Misc
{
    [TestFixture]
    public class PropertyComparerTests
    {
        #region SETUP

        private class TestObject
        {
            public string Name { get; set; }

            public decimal Rating { get; set; }
        }

        private PropertyComparer<TestObject> nameComparer = new PropertyComparer<TestObject>(x => x.Name);
        private PropertyComparer<TestObject> ratingComparer = new PropertyComparer<TestObject>(x => x.Rating);

        private PropertyComparer<TestObject> nameAndRatingComparer =
            new PropertyComparer<TestObject>(
                new Expression<Func<TestObject, object>>[] { x => x.Name, x => x.Rating }
            );

        #endregion SETUP

        [Test]
        public void Equals_WhenSameString_ExpectTrue()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1M };
            var t2 = new TestObject() { Name = "A", Rating = 1M };
            // Act
            var result = nameComparer.Equals(t1, t2);
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_WhenDifferentString_ExpectFalse()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1M };
            var t2 = new TestObject() { Name = "B", Rating = 1M };
            // Act
            var result = nameComparer.Equals(t1, t2);
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_WhenSameDecimal_ExpectTrue()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "A", Rating = 1.0M };
            // Act
            var result = ratingComparer.Equals(t1, t2);
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_WhenDifferentDecimal_ExpectFalse()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "A", Rating = 1.1M };
            // Act
            var result = ratingComparer.Equals(t1, t2);
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_WhenSameDecimal_ExpectSameHashCode()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 5.0M };
            var t2 = new TestObject() { Name = "A", Rating = 5.0M };
            // Act
            var result = ratingComparer.GetHashCode(t1);
            var result2 = ratingComparer.GetHashCode(t2);
            // Assert
            Assert.AreEqual(result, result2);
        }

        [Test]
        public void GetHashCode_WhenDifferentDecimal_ExpectDiffertHashCode()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 5.0M };
            var t2 = new TestObject() { Name = "A", Rating = 5.1M };
            // Act
            var result = ratingComparer.GetHashCode(t1);
            var result2 = ratingComparer.GetHashCode(t2);
            // Assert
            Assert.AreNotEqual(result, result2);
        }

        [Test]
        public void Equals_WhenSameStringAndDecimal_ExpectTrue()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "A", Rating = 1.0M };
            // Act
            var result = nameAndRatingComparer.Equals(t1, t2);
            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Equals_WhenSameStringAndDifferentDecimal_ExpectFalse()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "A", Rating = 1.1M };
            // Act
            var result = nameAndRatingComparer.Equals(t1, t2);
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_WhenDifferentStringAndSameDecimal_ExpectFalse()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "B", Rating = 1.0M };
            // Act
            var result = nameAndRatingComparer.Equals(t1, t2);
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Equals_WhenDifferentStringAndDecimal_ExpectFalse()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 1.0M };
            var t2 = new TestObject() { Name = "B", Rating = 1.1M };
            // Act
            var result = nameAndRatingComparer.Equals(t1, t2);
            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void GetHashCode_WhenSameStringAndDecimal_ExpectSameHashCode()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 5.0M };
            var t2 = new TestObject() { Name = "A", Rating = 5.0M };
            // Act
            var result = nameAndRatingComparer.GetHashCode(t1);
            var result2 = nameAndRatingComparer.GetHashCode(t2);
            // Assert
            Assert.AreEqual(result, result2);
        }

        [Test]
        public void GetHashCode_WhenDifferentStringAndDecimal_ExpectDiffertHashCode()
        {
            // Arrange
            var t1 = new TestObject() { Name = "A", Rating = 5.0M };
            var t2 = new TestObject() { Name = "B", Rating = 5.1M };
            // Act
            var result = nameAndRatingComparer.GetHashCode(t1);
            var result2 = nameAndRatingComparer.GetHashCode(t2);
            // Assert
            Assert.AreNotEqual(result, result2);
        }
    }
}
