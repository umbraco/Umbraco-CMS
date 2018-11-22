using System;
using System.Diagnostics;
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
            var item = new User()
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Comments = "comments",
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
                StartContentIds = new[] { 3 },
                StartMediaIds = new[] { 8 },
                Username = "username"
            };

            var clone = (User)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());

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

            var item = new User
            {
                Id = 3,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                Comments = "comments",
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
                StartContentIds = new[] { 3 },
                StartMediaIds = new[] { 8 },
                Username = "username"
            };

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }
    }
}
