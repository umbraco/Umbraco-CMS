using System;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;

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
    }
}