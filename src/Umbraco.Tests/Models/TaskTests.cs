using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class TaskTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new Task(new TaskType("test") {Id = 3})
            {
                AssigneeUserId = 4,
                Closed = true,
                Comment = "blah",
                CreateDate = DateTime.Now,
                EntityId = 99,
                Id = 2,
                Key = Guid.NewGuid(),
                OwnerUserId = 89,
                TaskType = new TaskType("asdf") {Id = 99},
                UpdateDate = DateTime.Now
            };

            var clone = (Task) item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);
            Assert.AreEqual(clone.AssigneeUserId, item.AssigneeUserId);
            Assert.AreEqual(clone.Closed, item.Closed);
            Assert.AreEqual(clone.Comment, item.Comment);
            Assert.AreEqual(clone.EntityId, item.EntityId);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.OwnerUserId, item.OwnerUserId);
            Assert.AreNotSame(clone.TaskType, item.TaskType);
            Assert.AreEqual(clone.TaskType, item.TaskType);

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

            var item = new Task(new TaskType("test") { Id = 3 })
            {
                AssigneeUserId = 4,
                Closed = true,
                Comment = "blah",
                CreateDate = DateTime.Now,
                EntityId = 99,
                Id = 2,
                Key = Guid.NewGuid(),
                OwnerUserId = 89,
                TaskType = new TaskType("asdf") { Id = 99 },
                UpdateDate = DateTime.Now
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

    }
}