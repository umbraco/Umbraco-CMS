using System;
using NUnit.Framework;
using Umbraco.Core.Models;
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

            var clone = (User)item.DeepClone();

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
                Password = "test pass",
                PasswordAnswer = "answer",
                PasswordQuestion = "question",
                //ProviderUserKey = "user key",
                SessionTimeout = 5,
                StartContentId = 3,
                StartMediaId = 8,
                Username = "username"                            
            };
          
            var clone = (User)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);

            Assert.AreNotSame(clone.UserType, item.UserType);
            Assert.AreEqual(clone.UserType, item.UserType);

            //Verify normal properties with reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }
    }

    [TestFixture]
    public class UmbracoEntityTests
    {
        [Test]
        public void UmbracoEntity_Can_Be_Initialized_From_Dynamic()
        {
            var boolIsTrue = true;
            var intIsTrue = 1;

            var trashedWithBool = new UmbracoEntity((dynamic)boolIsTrue);
            var trashedWithInt = new UmbracoEntity((dynamic)intIsTrue);

            Assert.IsTrue(trashedWithBool.Trashed);
            Assert.IsTrue(trashedWithInt.Trashed);
        }

        [Test]
        public void Can_Deep_Clone()
        {
            var item = new UmbracoEntity()
            {
                Id = 3,
                ContentTypeAlias = "test1",
                CreatorId = 4,
                Key = Guid.NewGuid(),
                UpdateDate = DateTime.Now,
                CreateDate = DateTime.Now,
                Name = "Test",
                ParentId = 5,
                SortOrder = 6,
                Path = "-1,23",
                Level = 7,
                ContentTypeIcon = "icon",
                ContentTypeThumbnail = "thumb",
                HasChildren = true,
                HasPendingChanges = true,
                IsDraft = true,
                IsPublished = true,
                NodeObjectTypeId = Guid.NewGuid()            
            };
            item.AdditionalData.Add("test1", 3);
            item.AdditionalData.Add("test2", "valuie");
            item.UmbracoProperties.Add(new UmbracoEntity.UmbracoProperty()
            {
                Value = "test",
                DataTypeControlId = Guid.NewGuid()
            });
            item.UmbracoProperties.Add(new UmbracoEntity.UmbracoProperty()
            {
                Value = "test2",
                DataTypeControlId = Guid.NewGuid()
            });

            var clone = (UmbracoEntity)item.DeepClone();

            Assert.AreNotSame(clone, item);
            Assert.AreEqual(clone, item);
            Assert.AreEqual(clone.CreateDate, item.CreateDate);
            Assert.AreEqual(clone.ContentTypeAlias, item.ContentTypeAlias);
            Assert.AreEqual(clone.CreatorId, item.CreatorId);
            Assert.AreEqual(clone.Id, item.Id);
            Assert.AreEqual(clone.Key, item.Key);
            Assert.AreEqual(clone.Level, item.Level);            
            Assert.AreEqual(clone.Name, item.Name);
            Assert.AreEqual(clone.ParentId, item.ParentId);
            Assert.AreEqual(clone.SortOrder, item.SortOrder);
            Assert.AreEqual(clone.Path, item.Path);
            Assert.AreEqual(clone.ContentTypeIcon, item.ContentTypeIcon);
            Assert.AreEqual(clone.ContentTypeThumbnail, item.ContentTypeThumbnail);
            Assert.AreEqual(clone.HasChildren, item.HasChildren);
            Assert.AreEqual(clone.HasPendingChanges, item.HasPendingChanges);
            Assert.AreEqual(clone.IsDraft, item.IsDraft);
            Assert.AreEqual(clone.IsPublished, item.IsPublished);
            Assert.AreEqual(clone.NodeObjectTypeId, item.NodeObjectTypeId);
            Assert.AreEqual(clone.UpdateDate, item.UpdateDate);
            Assert.AreEqual(clone.AdditionalData.Count, item.AdditionalData.Count);
            Assert.AreEqual(clone.AdditionalData, item.AdditionalData);
            Assert.AreEqual(clone.UmbracoProperties.Count, item.UmbracoProperties.Count);
            for (var i = 0; i < clone.UmbracoProperties.Count; i++)
            {
                Assert.AreNotSame(clone.UmbracoProperties[i], item.UmbracoProperties[i]);
                Assert.AreEqual(clone.UmbracoProperties[i], item.UmbracoProperties[i]);
            }
            
            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
            }
        }
    }
}