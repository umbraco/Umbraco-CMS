using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class MemberGroupTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            // Arrange
            var group = new MemberGroup()
            {
                CreateDate = DateTime.Now,
                CreatorId = 4,
                Id = 6,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                Name = "asdf"
            };
            group.AdditionalData.Add("test1", 123);
            group.AdditionalData.Add("test2", "hello");

            // Act
            var clone = (MemberGroup)group.DeepClone();

            // Assert
            Assert.AreNotSame(clone, group);
            Assert.AreEqual(clone, group);
            Assert.AreEqual(clone.Id, group.Id);
            Assert.AreEqual(clone.AdditionalData, group.AdditionalData);
            Assert.AreEqual(clone.AdditionalData.Count, group.AdditionalData.Count);
            Assert.AreEqual(clone.CreateDate, group.CreateDate);
            Assert.AreEqual(clone.CreatorId, group.CreatorId);
            Assert.AreEqual(clone.Key, group.Key);
            Assert.AreEqual(clone.UpdateDate, group.UpdateDate);
            Assert.AreEqual(clone.Name, group.Name);
            
            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(group, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var group = new MemberGroup()
            {
                CreateDate = DateTime.Now,
                CreatorId = 4,
                Id = 6,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                Name = "asdf"
            };
            group.AdditionalData.Add("test1", 123);
            group.AdditionalData.Add("test2", "hello");

            var result = ss.ToStream(group);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

    }
}