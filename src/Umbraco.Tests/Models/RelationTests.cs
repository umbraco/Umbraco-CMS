using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class RelationTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new Relation(9, 8, new RelationType(Guid.NewGuid(), Guid.NewGuid(), "test")
            {
                Id = 66
            })
            {
                Comment = "test comment",
                CreateDate = DateTime.Now,
                Id = 4,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now
            };

            var clone = (Relation) item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.ChildId, item.ChildId);
            Assert.AreEqual(clone.Comment, item.Comment);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.ParentId, item.ParentId);
            Assert.AreNotSame(clone.RelationType, item.RelationType);
            Assert.AreEqual(clone.RelationType, item.RelationType);
            Assert.AreEqual(clone.RelationTypeId, item.RelationTypeId);
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

            var item = new Relation(9, 8, new RelationType(Guid.NewGuid(), Guid.NewGuid(), "test")
            {
                Id = 66
            })
            {
                Comment = "test comment",
                CreateDate = DateTime.Now,
                Id = 4,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}