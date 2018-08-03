using System;
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Serialization;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class UmbracoEntityTests
    {
        [Test]
        public void Validate_Path()
        {
            var entity = new UmbracoEntity();

            //it's empty with no id so we need to allow it
            Assert.IsTrue(entity.ValidatePath());

            entity.Id = 1234;

            //it has an id but no path, so we can't allow it
            Assert.IsFalse(entity.ValidatePath());

            entity.Path = "-1";

            //invalid path
            Assert.IsFalse(entity.ValidatePath());
            
            entity.Path = string.Concat("-1,", entity.Id);

            //valid path
            Assert.IsTrue(entity.ValidatePath());
        }

        [Test]
        public void Ensure_Path_Throws_Without_Id()
        {
            var entity = new UmbracoEntity();

            //no id assigned
            Assert.Throws<InvalidOperationException>(() => entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => new UmbracoEntity(), umbracoEntity => { }));
        }

        [Test]
        public void Ensure_Path_Throws_Without_Parent()
        {
            var entity = new UmbracoEntity {Id = 1234};

            //no parent found
            Assert.Throws<NullReferenceException>(() => entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => null, umbracoEntity => { }));            
        }

        [Test]
        public void Ensure_Path_Entity_At_Root()
        {
            var entity = new UmbracoEntity
            {
                Id = 1234,
                ParentId = -1
            };


            entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => null, umbracoEntity => { });

            //works because it's under the root
            Assert.AreEqual("-1,1234", entity.Path);
        }

        [Test]
        public void Ensure_Path_Entity_Valid_Parent()
        {
            var entity = new UmbracoEntity
            {
                Id = 1234,
                ParentId = 888
            };

            entity.EnsureValidPath(Mock.Of<ILogger>(), umbracoEntity => umbracoEntity.ParentId == 888 ? new UmbracoEntity{Id = 888, Path = "-1,888"} : null, umbracoEntity => { });

            //works because the parent was found
            Assert.AreEqual("-1,888,1234", entity.Path);
        }

        [Test]
        public void Ensure_Path_Entity_Valid_Recursive_Parent()
        {
            var parentA = new UmbracoEntity
            {
                Id = 999,
                ParentId = -1
            };

            var parentB = new UmbracoEntity
            {
                Id = 888,
                ParentId = 999
            };

            var parentC = new UmbracoEntity
            {
                Id = 777,
                ParentId = 888
            };

            var entity = new UmbracoEntity
            {
                Id = 1234,
                ParentId = 777
            };

            Func<IUmbracoEntity, IUmbracoEntity> getParent = umbracoEntity =>
            {
                switch (umbracoEntity.ParentId)
                {
                    case 999:
                        return parentA;
                    case 888:
                        return parentB;
                    case 777:
                        return parentC;
                    case 1234:
                        return entity;
                    default:
                        return null;
                }
            };
            
            //this will recursively fix all paths
            entity.EnsureValidPath(Mock.Of<ILogger>(), getParent, umbracoEntity => { });
            
            Assert.AreEqual("-1,999", parentA.Path);
            Assert.AreEqual("-1,999,888", parentB.Path);
            Assert.AreEqual("-1,999,888,777", parentC.Path);
            Assert.AreEqual("-1,999,888,777,1234", entity.Path);            
        }

        [Test]
        public void UmbracoEntity_Can_Be_Initialized_From_Dynamic()
        {
            var boolIsTrue = true;
            ulong ulongIsTrue = 1; // because MySql might return ulong

            var trashedWithBool = new UmbracoEntity((dynamic)boolIsTrue);
            var trashedWithInt = new UmbracoEntity((dynamic)ulongIsTrue);

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

            item.AdditionalData.Add("test3", new UmbracoEntity.EntityProperty()
            {
                Value = "test",
                PropertyEditorAlias = "TestPropertyEditor"
            });
            item.AdditionalData.Add("test4", new UmbracoEntity.EntityProperty()
            {
                Value = "test2",
                PropertyEditorAlias = "TestPropertyEditor2"
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
            item.AdditionalData.Add("test3", new UmbracoEntity.EntityProperty()
            {
                Value = "test",
                PropertyEditorAlias = "TestPropertyEditor"
            });
            item.AdditionalData.Add("test4", new UmbracoEntity.EntityProperty()
            {
                Value = "test2",
                PropertyEditorAlias = "TestPropertyEditor2"
            });

            var result = ss.ToStream(item);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }
    }
}