using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class MemberTests
    {
        [Test]
        public void Can_Deep_Clone()
        {
            // Arrange
            var memberType = MockedContentTypes.CreateSimpleMemberType("memberType", "Member Type");
            memberType.Id = 99;
            var member = MockedMember.CreateSimpleMember(memberType, "Name", "email@email.com", "pass", "user", Guid.NewGuid());
            var i = 200;
            foreach (var property in member.Properties)
            {
                property.Id = ++i;
            }
            member.Id = 10;
            member.CreateDate = DateTime.Now;
            member.CreatorId = 22;
            member.Comments = "comments";
            member.Key = Guid.NewGuid();
            member.FailedPasswordAttempts = 22;
            member.Level = 3;
            member.Path = "-1,4,10";
            member.Groups = new[] {"group1", "group2"};
            member.IsApproved = true;
            member.IsLockedOut = true;
            member.LastLockoutDate = DateTime.Now;
            member.LastLoginDate = DateTime.Now;
            member.LastPasswordChangeDate = DateTime.Now;
            member.PasswordQuestion = "question";
            member.ProviderUserKey = Guid.NewGuid();
            member.RawPasswordAnswerValue = "raw answer";
            member.RawPasswordValue = "raw pass";
            member.SortOrder = 5;
            member.Trashed = false;
            member.UpdateDate = DateTime.Now;
            member.Version = Guid.NewGuid();            
            ((IUmbracoEntity)member).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)member).AdditionalData.Add("test2", "hello");

            // Act
            var clone = (Member)member.DeepClone();

            // Assert
            Assert.AreNotSame(clone, member);
            Assert.AreEqual(clone, member);
            Assert.AreEqual(clone.Id, member.Id);
            Assert.AreEqual(clone.Version, member.Version);
            Assert.AreEqual(((IUmbracoEntity)clone).AdditionalData, ((IUmbracoEntity)member).AdditionalData);
            Assert.AreNotSame(clone.ContentType, member.ContentType);
            Assert.AreEqual(clone.ContentType, member.ContentType);
            Assert.AreEqual(clone.ContentType.PropertyGroups.Count, member.ContentType.PropertyGroups.Count);
            for (var index = 0; index < member.ContentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.ContentType.PropertyGroups[index], member.ContentType.PropertyGroups[index]);
                Assert.AreEqual(clone.ContentType.PropertyGroups[index], member.ContentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.ContentType.PropertyTypes.Count(), member.ContentType.PropertyTypes.Count());
            for (var index = 0; index < member.ContentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.ContentType.PropertyTypes.ElementAt(index), member.ContentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.ContentType.PropertyTypes.ElementAt(index), member.ContentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.ContentTypeId, member.ContentTypeId);
            Assert.AreEqual(clone.CreateDate, member.CreateDate);
            Assert.AreEqual(clone.CreatorId, member.CreatorId);
            Assert.AreEqual(clone.Comments, member.Comments);
            Assert.AreEqual(clone.Key, member.Key);
            Assert.AreEqual(clone.FailedPasswordAttempts, member.FailedPasswordAttempts);
            Assert.AreEqual(clone.Level, member.Level);
            Assert.AreEqual(clone.Path, member.Path);
            Assert.AreEqual(clone.Groups, member.Groups);
            Assert.AreEqual(clone.Groups.Count(), member.Groups.Count());
            Assert.AreEqual(clone.IsApproved, member.IsApproved);
            Assert.AreEqual(clone.IsLockedOut, member.IsLockedOut);
            Assert.AreEqual(clone.SortOrder, member.SortOrder);
            Assert.AreEqual(clone.LastLockoutDate, member.LastLockoutDate);
            Assert.AreNotSame(clone.LastLoginDate, member.LastLoginDate);
            Assert.AreEqual(clone.LastPasswordChangeDate, member.LastPasswordChangeDate);
            Assert.AreEqual(clone.Trashed, member.Trashed);
            Assert.AreEqual(clone.UpdateDate, member.UpdateDate);
            Assert.AreEqual(clone.Version, member.Version);
            Assert.AreEqual(clone.PasswordQuestion, member.PasswordQuestion);
            Assert.AreEqual(clone.ProviderUserKey, member.ProviderUserKey);
            Assert.AreEqual(clone.RawPasswordAnswerValue, member.RawPasswordAnswerValue);
            Assert.AreEqual(clone.RawPasswordValue, member.RawPasswordValue);
            Assert.AreNotSame(clone.Properties, member.Properties);
            Assert.AreEqual(clone.Properties.Count(), member.Properties.Count());
            for (var index = 0; index < member.Properties.Count; index++)
            {
                Assert.AreNotSame(clone.Properties[index], member.Properties[index]);
                Assert.AreEqual(clone.Properties[index], member.Properties[index]);
            }

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(member, null));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            var memberType = MockedContentTypes.CreateSimpleMemberType("memberType", "Member Type");
            memberType.Id = 99;
            var member = MockedMember.CreateSimpleMember(memberType, "Name", "email@email.com", "pass", "user", Guid.NewGuid());
            var i = 200;
            foreach (var property in member.Properties)
            {
                property.Id = ++i;
            }
            member.Id = 10;
            member.CreateDate = DateTime.Now;
            member.CreatorId = 22;
            member.Comments = "comments";
            member.Key = Guid.NewGuid();
            member.FailedPasswordAttempts = 22;
            member.Level = 3;
            member.Path = "-1,4,10";
            member.Groups = new[] { "group1", "group2" };
            member.IsApproved = true;
            member.IsLockedOut = true;
            member.LastLockoutDate = DateTime.Now;
            member.LastLoginDate = DateTime.Now;
            member.LastPasswordChangeDate = DateTime.Now;
            member.PasswordQuestion = "question";
            member.ProviderUserKey = Guid.NewGuid();
            member.RawPasswordAnswerValue = "raw answer";
            member.RawPasswordValue = "raw pass";
            member.SortOrder = 5;
            member.Trashed = false;
            member.UpdateDate = DateTime.Now;
            member.Version = Guid.NewGuid();
            ((IUmbracoEntity)member).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)member).AdditionalData.Add("test2", "hello");

            var result = ss.ToStream(member);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}