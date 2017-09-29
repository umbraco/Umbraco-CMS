using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Facade
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
        public void ModelTypeMapTests()
        {
            var map = new Dictionary<string, Type>
            {
                { "alias1", typeof (FacadeTestObjects.TestElementModel1) },
                { "alias2", typeof (FacadeTestObjects.TestElementModel2) },
            };

            Assert.AreEqual("Umbraco.Tests.Facade.FacadeTestObjects+TestElementModel1",
                ModelType.Map(ModelType.For("alias1"), map).ToString());
            Assert.AreEqual("Umbraco.Tests.Facade.FacadeTestObjects+TestElementModel1[]",
                ModelType.Map(ModelType.For("alias1").MakeArrayType(), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.Facade.FacadeTestObjects+TestElementModel1]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1")), map).ToString());
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[Umbraco.Tests.Facade.FacadeTestObjects+TestElementModel1[]]",
                ModelType.Map(typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()), map).ToString());
        }
    }
}