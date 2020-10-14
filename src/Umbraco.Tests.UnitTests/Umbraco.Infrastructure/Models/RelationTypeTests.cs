using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class RelationTypeTests
    {
        private RelationTypeBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new RelationTypeBuilder();
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var item = _builder
                .WithId(1)
                .WithParentObjectType(Guid.NewGuid())
                .WithChildObjectType(Guid.NewGuid())
                .Build();

            var clone = (RelationType) item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.Alias, item.Alias);
            Assert.AreEqual(clone.ChildObjectType, item.ChildObjectType);
            Assert.AreEqual(clone.ParentObjectType, item.ParentObjectType);
            Assert.AreEqual(clone.IsBidirectional, item.IsBidirectional);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.Name, item.Name);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var item = _builder.Build();

            Assert.DoesNotThrow(() => JsonConvert.SerializeObject(item));
        }
    }
}
