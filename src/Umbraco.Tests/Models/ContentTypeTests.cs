using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTypeTests : BaseUmbracoConfigurationTest
    {
       

        [Test]
        public void Can_Deep_Clone_Content_Type_Sort()
        {
            var contentType = new ContentTypeSort(new Lazy<int>(() => 3), 4, "test");
            var clone = (ContentTypeSort) contentType.DeepClone();
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id.Value, contentType.Id.Value);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Alias, contentType.Alias);

        }

        [Test]
        public void Can_Deep_Clone_Content_Type_With_Reset_Identities()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            //add a property type without a property group
            contentType.PropertyTypeCollection.Add(
                new PropertyType("test", DataTypeDatabaseType.Ntext, "title2") { Name = "Title2", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });

            contentType.AllowedTemplates = new[] { new Template("-1,2", "Name", "name") { Id = 200 }, new Template("-1,3", "Name2", "name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);

            var clone = (ContentType)contentType.Clone("newAlias");

            Assert.AreEqual("newAlias", clone.Alias);
            Assert.AreNotEqual("newAlias", contentType.Alias);
            Assert.IsFalse(clone.HasIdentity);

            foreach (var propertyGroup in clone.PropertyGroups)
            {
                Assert.IsFalse(propertyGroup.HasIdentity);
                foreach (var propertyType in propertyGroup.PropertyTypes)
                {
                    Assert.IsFalse(propertyType.HasIdentity);
                }
            }

            foreach (var propertyType in clone.PropertyTypes.Where(x => x.HasIdentity))
            {
                Assert.IsFalse(propertyType.HasIdentity);
            }
        }

        [Ignore]
        [Test]
        public void Can_Deep_Clone_Content_Type_Perf_Test()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template("-1,2", "Name", "name") { Id = 200 }, new Template("-1,3", "Name2", "name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            using (DisposableTimer.DebugDuration<ContentTypeTests>("STARTING PERF TEST"))
            {
                for (int j = 0; j < 1000; j++)
                {
                    using (DisposableTimer.DebugDuration<ContentTypeTests>("Cloning content type"))
                    {
                        var clone = (ContentType)contentType.DeepClone();
                    }                    
                }
            }
        }

        [Test]
        public void Can_Deep_Clone_Content_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.Id = 99;
            
            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            foreach (var group in contentType.PropertyGroups)
            {
                group.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template("-1,2", "Name", "name") { Id = 200 }, new Template("-1,3", "Name2", "name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] {new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2")};
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            });            
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;            
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            // Act
            var clone = (ContentType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(((IUmbracoEntity)clone).AdditionalData, ((IUmbracoEntity)contentType).AdditionalData);
            Assert.AreEqual(clone.AllowedTemplates.Count(), contentType.AllowedTemplates.Count());
            for (var index = 0; index < contentType.AllowedTemplates.Count(); index++)
            {
                Assert.AreNotSame(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
                Assert.AreEqual(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
            }
            Assert.AreNotSame(clone.PropertyGroups, contentType.PropertyGroups);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreNotSame(clone.PropertyTypes, contentType.PropertyTypes);
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            Assert.AreEqual(0, ((ContentTypeBase)clone).NonGroupedPropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }

            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreNotSame(clone.DefaultTemplate, contentType.DefaultTemplate);
            Assert.AreEqual(clone.DefaultTemplate, contentType.DefaultTemplate);
            Assert.AreEqual(clone.DefaultTemplateId, contentType.DefaultTemplateId);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);
            
            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }

            //need to ensure the event handlers are wired

            var asDirty = (ICanBeDirty)clone;

            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyTypes"));
            clone.AddPropertyType(new PropertyType("test", DataTypeDatabaseType.Nvarchar, "blah"));
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyTypes"));
            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyGroups"));
            clone.AddPropertyGroup("hello");
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyGroups"));
        }

        [Test]
        public void Can_Serialize_Content_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.AllowedTemplates = new[] { new Template("-1,2", "Name", "name") { Id = 200 }, new Template("-1,3", "Name2", "name2") { Id = 201 } };
            contentType.AllowedContentTypes = new[] { new ContentTypeSort(new Lazy<int>(() => 888), 8, "sub"), new ContentTypeSort(new Lazy<int>(() => 889), 9, "sub2") };
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.SetDefaultTemplate(new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            });
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

        [Test]
        public void Can_Deep_Clone_Media_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateImageMediaType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;            
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            // Act
            var clone = (MediaType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(((IUmbracoEntity)clone).AdditionalData, ((IUmbracoEntity)contentType).AdditionalData);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }
        }

        [Test]
        public void Can_Serialize_Media_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateImageMediaType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;

            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }

        [Test]
        public void Can_Deep_Clone_Member_Type()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateSimpleMemberType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;
            contentType.SetMemberCanEditProperty("title", true);
            contentType.SetMemberCanViewProperty("bodyText", true);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            // Act
            var clone = (MemberType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
            Assert.AreEqual(((IUmbracoEntity)clone).AdditionalData, ((IUmbracoEntity)contentType).AdditionalData);
            Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
            for (var index = 0; index < contentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
                Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
            for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
            Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
            Assert.AreEqual(clone.Key, contentType.Key);
            Assert.AreEqual(clone.Level, contentType.Level);
            Assert.AreEqual(clone.Path, contentType.Path);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);
            Assert.AreEqual(clone.MemberTypePropertyTypes, contentType.MemberTypePropertyTypes);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
            }
        }

        [Test]
        public void Can_Serialize_Member_Type_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateSimpleMemberType();
            contentType.Id = 99;

            var i = 200;
            foreach (var propertyType in contentType.PropertyTypes)
            {
                propertyType.Id = ++i;
            }
            contentType.Id = 10;
            contentType.CreateDate = DateTime.Now;
            contentType.CreatorId = 22;
            contentType.Description = "test";
            contentType.Icon = "icon";
            contentType.IsContainer = true;
            contentType.Thumbnail = "thumb";
            contentType.Key = Guid.NewGuid();
            contentType.Level = 3;
            contentType.Path = "-1,4,10";
            contentType.SortOrder = 5;
            contentType.Trashed = false;
            contentType.UpdateDate = DateTime.Now;
            contentType.SetMemberCanEditProperty("title", true);
            contentType.SetMemberCanViewProperty("bodyText", true);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)contentType).AdditionalData.Add("test2", "hello");

            var result = ss.ToStream(contentType);
            var json = result.ResultStream.ToJsonString();
            Console.WriteLine(json);
        }
    }
}