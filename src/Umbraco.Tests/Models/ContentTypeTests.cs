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
    public class ContentTypeTests
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