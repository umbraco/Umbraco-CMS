using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UserTypeTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new UserType()
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Alias = "test",
                Permissions = new[] {"a", "b", "c"}                
            };

            var clone = (UserType)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            
            //Verify normal properties with reflection
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

            var item = new UserType()
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Alias = "test",
                Permissions = new[] { "a", "b", "c" }
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}