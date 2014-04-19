using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class RelationTypeTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new RelationType(Guid.NewGuid(), Guid.NewGuid(), "test")
            {
                Id = 66,
                CreateDate = DateTime.Now,
                IsBidirectional = true,
                Key = Guid.NewGuid(),
                Name = "Test",
                UpdateDate = DateTime.Now                
            };

            var clone = (RelationType)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.Alias, item.Alias);
            Assert.AreEqual(clone.ChildObjectType, item.ChildObjectType);
            Assert.AreEqual(clone.IsBidirectional, item.IsBidirectional);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.Name, item.Name);
            Assert.AreNotSame(clone.ParentObjectType, item.ParentObjectType);            
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
            var ss = new SerializationService(new JsonNetSerializer());

            var item = new RelationType(Guid.NewGuid(), Guid.NewGuid(), "test")
            {
                Id = 66,
                CreateDate = DateTime.Now,
                IsBidirectional = true,
                Key = Guid.NewGuid(),
                Name = "Test",
                UpdateDate = DateTime.Now
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}