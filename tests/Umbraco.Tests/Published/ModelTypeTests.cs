using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Tests.Published
{
    [TestFixture]
    public class ModelTypeTests
    {

            //TODO these is not easy to move to the Unittest project due to underlysing NotImplementedException of Type.IsSZArray
        [Test]
        public void ModelTypeToStringTests()
        {
            var modelType = ModelType.For("alias1");
            var modelTypeArray = modelType.MakeArrayType();

            Assert.AreEqual("{alias1}", modelType.ToString());

            // there's an "*" there because the arrays are not true SZArray - but that changes when we map

            Assert.AreEqual("{alias1}[*]", modelTypeArray.ToString());
            var enumArray = typeof(IEnumerable<>).MakeGenericType(modelTypeArray);
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[{alias1}[*]]", enumArray.ToString());
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
            Assert.AreEqual("System.Collections.Generic.IEnumerable`1[[{alias1}[*], Umbraco.Core, Version=0.5.0.0, Culture=neutral, PublicKeyToken=null]]", typeof(IEnumerable<>).MakeGenericType(ModelType.For("alias1").MakeArrayType()).FullName);
        }

    }
}
