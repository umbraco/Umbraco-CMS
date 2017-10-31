using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Published
{
    [TestFixture]
    public class ModelTypeTests
    {
        [Test]
        public void ModelTypeEqualityTests()
        {
            Assert.AreNotEqual(ModelType.For("alias1"), ModelType.For("alias1"));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias1")));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1"), ModelType.For("alias2")));

            Assert.IsTrue(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1"))));
            Assert.IsFalse(ModelType.Equals(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias2"))));

            Assert.IsTrue(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias1").MakeArrayType()));
            Assert.IsFalse(ModelType.Equals(ModelType.For("alias1").MakeArrayType(), ModelType.For("alias2").MakeArrayType()));
        }

        [Test]
        public void ModelTypeToStringTests()
        {
            Assert.AreEqual("{alias1}", ModelType.For("alias1").ToString());

            // there's an "*" there because the arrays are not true SZArray - but that changes when we map
            Assert.AreEqual("{alias1}[*]", ModelType.For("alias1").MakeArrayType().ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[{alias1}[*]]", typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()).ToString());
        }

        [Test]
        public void TypeToStringTests()
        {
            var type = typeof(int);
            Assert.AreEqual("System.Int32", type.ToString());
            Assert.AreEqual("System.Int32[]", type.MakeArrayType().ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[System.Int32[]]", typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).ToString());
        }

        [Test]
        public void ModelTypeFullNameTests()
        {
            Assert.AreEqual("{alias1}", ModelType.For("alias1").FullName);

            Type type = ModelType.For("alias1");
            Assert.AreEqual("{alias1}", type.FullName);

            // there's an "*" there because the arrays are not true SZArray - but that changes when we map
            Assert.AreEqual("{alias1}[*]", ModelType.For("alias1").MakeArrayType().FullName);
            // note the inner assembly qualified name
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[[{alias1}[*], Umbraco.Core, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null]]", typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()).FullName);
        }

        [Test]
        public void TypeFullNameTests()
        {
            var type = typeof(int);
            Assert.AreEqual("System.Int32", type.FullName);
            Assert.AreEqual("System.Int32[]", type.MakeArrayType().FullName);
            // note the inner assembly qualified name
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[[System.Int32[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", typeof(IEnumerable<>).MakeGenericType(type.MakeArrayType()).FullName);
        }

        [Test]
        public void ModelTypeMapTests()
        {
            var map = new Dictionary<string, Type>
            {
                { "alias1", typeof (PublishedSnapshotTestObjects.TestElementModel1) },
                { "alias2", typeof (PublishedSnapshotTestObjects.TestElementModel2) },
            };

            Assert.AreEqual("Umbraco.Tests.Published.PublishedSnapshotTestObjects+TestElementModel1",
                ModelType.Map(ModelType.For("alias1"), map).ToString());
            Assert.AreEqual("Umbraco.Tests.Published.PublishedSnapshotTestObjects+TestElementModel1[]",
                ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.Published.PublishedSnapshotTestObjects+TestElementModel1]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.Published.PublishedSnapshotTestObjects+TestElementModel1[]]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map).ToString());
        }
    }
}