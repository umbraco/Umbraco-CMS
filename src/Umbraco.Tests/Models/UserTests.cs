using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            var item = new User(new UserType(){Id = 3})
            {
                Id = 3,                
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Comments = "comments",
                DefaultPermissions = new[]{"a","b","c"},
                DefaultToLiveEditing = false,
                Email = "test@test.com",
                Language = "en",
                FailedPasswordAttempts = 3,
                IsApproved = true,
                IsLockedOut = true,
                LastLockoutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                //Password = "test pass",
                //PasswordAnswer = "answer",
                PasswordQuestion = "question",
                //ProviderUserKey = "user key",
                SessionTimeout = 5,
                StartContentId = 3,
                StartMediaId = 8,
                Username = "username"                            
            };
          
            item.AddAllowedSection("test");

            var clone = (User)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreNotSame(clone.UserType, item.UserType);
            Assert.AreEqual(clone.UserType, item.UserType);
            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());

            Assert.AreNotSame(clone.DefaultPermissions, item.DefaultPermissions);
            Assert.AreEqual(clone.DefaultPermissions.Count(), item.DefaultPermissions.Count());

            //Verify normal properties with reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }

            //ensure internal collections are differet
            Assert.AreNotSame(item.AddedSections, clone.AddedSections);
            Assert.AreNotSame(item.RemovedSections, clone.RemovedSections);

            //ensure event handlers are still wired on clone
            clone.AddAllowedSection("blah");
            Assert.AreEqual(1, clone.AddedSections.Count());
            clone.RemoveAllowedSection("blah");
            Assert.AreEqual(1, clone.RemovedSections.Count());

        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var item = new User(new UserType() { Id = 3 })
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Comments = "comments",
                DefaultPermissions = new[] { "a", "b", "c" },
                DefaultToLiveEditing = false,
                Email = "test@test.com",
                Language = "en",
                FailedPasswordAttempts = 3,
                IsApproved = true,
                IsLockedOut = true,
                LastLockoutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                //Password = "test pass",
                //PasswordAnswer = "answer",
                PasswordQuestion = "question",
                //ProviderUserKey = "user key",
                SessionTimeout = 5,
                StartContentId = 3,
                StartMediaId = 8,
                Username = "username"
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}