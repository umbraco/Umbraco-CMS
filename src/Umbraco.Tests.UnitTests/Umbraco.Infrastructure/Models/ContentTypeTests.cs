using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.Models
{
    [TestFixture]
    public class ContentTypeTests
    {
        [Test]
        [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
        public void Cannot_Add_Duplicate_Property_Aliases()
        {
            var contentType = (ContentType)BuildContentType();

            var propertyTypeBuilder = new PropertyTypeBuilder();
            var additionalPropertyType = propertyTypeBuilder
                .WithAlias("title")
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                contentType.PropertyTypeCollection.Add(additionalPropertyType));
        }

        [Test]
        [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
        public void Cannot_Update_Duplicate_Property_Aliases()
        {
            var contentType = (ContentType)BuildContentType();

            var propertyTypeBuilder = new PropertyTypeBuilder();
            var additionalPropertyType = propertyTypeBuilder
                .WithAlias("title")
                .Build();

            contentType.PropertyTypeCollection.Add(additionalPropertyType);

            var toUpdate = contentType.PropertyTypeCollection["myPropertyType2"];

            Assert.Throws<InvalidOperationException>(() => toUpdate.Alias = "myPropertyType");
        }

        [Test]
        public void Can_Deep_Clone_Content_Type_Sort()
        {
            var contentType = BuildContentTypeSort();
            var clone = (ContentTypeSort)contentType.DeepClone();
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id.Value, contentType.Id.Value);
            Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
            Assert.AreEqual(clone.Alias, contentType.Alias);
        }

        private ContentTypeSort BuildContentTypeSort()
        {
            var builder = new ContentTypeSortBuilder();
            return builder
                .WithId(3)
                .WithSortOrder(4)
                .WithAlias("test")
                .Build();
        }

        [Test]
        public void Can_Deep_Clone_Content_Type_With_Reset_Identities()
        {
            var contentType = BuildContentType();

            var clone = (ContentType)contentType.DeepCloneWithResetIdentities("newAlias");

            Assert.AreEqual("newAlias", clone.Alias);
            Assert.AreNotEqual("newAlias", contentType.Alias);
            Assert.IsFalse(clone.HasIdentity);

            foreach (var propertyGroup in clone.PropertyGroups)
            {
                Assert.IsFalse(propertyGroup.HasIdentity);
                foreach (var propertyType in propertyGroup.PropertyTypes)
                    Assert.IsFalse(propertyType.HasIdentity);
            }

            foreach (var propertyType in clone.PropertyTypes.Where(x => x.HasIdentity))
                Assert.IsFalse(propertyType.HasIdentity);
        }        

        [Test]
        public void Can_Deep_Clone_Content_Type()
        {
            // Arrange
            var contentType = BuildContentType();

            // Act
            var clone = (ContentType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
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
            Assert.AreEqual(0, clone.NoGroupPropertyTypes.Count());
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
            Assert.AreEqual(clone.DefaultTemplateId, ((ContentType)contentType).DefaultTemplateId);
            Assert.AreEqual(clone.Trashed, contentType.Trashed);
            Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
            Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
            Assert.AreEqual(clone.Icon, contentType.Icon);
            Assert.AreEqual(clone.IsContainer, contentType.IsContainer);

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));

            //need to ensure the event handlers are wired

            var asDirty = (ICanBeDirty)clone;

            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyTypes"));

            var propertyTypeBuilder = new PropertyTypeBuilder();
            var additionalPropertyType = propertyTypeBuilder
                .WithPropertyEditorAlias("test")
                .WithValueStorageType(ValueStorageType.Nvarchar)
                .WithAlias("blah")
                .Build();

            clone.AddPropertyType(additionalPropertyType);
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyTypes"));
            Assert.IsFalse(asDirty.IsPropertyDirty("PropertyGroups"));
            clone.AddPropertyGroup("hello");
            Assert.IsTrue(asDirty.IsPropertyDirty("PropertyGroups"));
        }

        [Test]
        public void Can_Serialize_Content_Type_Without_Error()
        {
            // Arrange
            var contentType = BuildContentType();

            var json = JsonConvert.SerializeObject(contentType);
            Debug.Print(json);
        }

        private static IContentType BuildContentType()
        {
            var builder = new ContentTypeBuilder();
            return builder
                .WithId(10)
                .WithAlias("textPage")
                .WithName("Text Page")
                .WithCreatorId(22)
                .WithDescription("test")
                .WithIsContainer(true)
                .WithIcon("icon")
                .WithThumbnail("thumb")
                .WithSortOrder(5)
                .WithLevel(3)
                .WithPath("-1,4,10")
                .WithPropertyTypeIdsIncrementingFrom(200)
                .AddPropertyGroup()
                    .WithName("Content")
                    .WithSortOrder(1)
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Nvarchar)
                        .WithAlias("title")
                        .WithName("Title")
                        .WithSortOrder(1)
                        .WithDataTypeId(-88)
                        .Done()
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Ntext)
                        .WithAlias("bodyText")
                        .WithName("Body text")
                        .WithSortOrder(2)
                        .WithDataTypeId(-87)
                        .Done()
                    .Done()
                .AddPropertyGroup()
                    .WithName("Meta")
                    .WithSortOrder(2)
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Nvarchar)
                        .WithAlias("keywords")
                        .WithName("Keywords")
                        .WithSortOrder(1)
                        .WithDataTypeId(-88)
                        .Done()
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Nvarchar)
                        .WithAlias("description")
                        .WithName("description")
                        .WithSortOrder(1)
                        .WithDataTypeId(-88)
                        .Done()
                    .Done()
                .AddAllowedTemplate()
                    .WithId(200)
                    .WithAlias("textPage")
                    .WithName("Text Page")
                    .Done()
                .AddAllowedTemplate()
                    .WithId(201)
                    .WithAlias("textPage2")
                    .WithName("Text Page 2")
                    .Done()
                .WithDefaultTemplateId(200)
                .AddAllowedContentType()
                    .WithId(888)
                    .WithAlias("sub")
                    .WithSortOrder(8)
                    .Done()
                .AddAllowedContentType()
                    .WithId(889)
                    .WithAlias("sub2")
                    .WithSortOrder(9)
                    .Done()
                .Build();
        }

        [Test]
        public void Can_Deep_Clone_Media_Type()
        {
            // Arrange
            var contentType = BuildMediaType();

            // Act
            var clone = (MediaType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
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
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
        }

        [Test]
        public void Can_Serialize_Media_Type_Without_Error()
        {
            // Arrange
            var contentType = BuildMediaType();

            var json = JsonConvert.SerializeObject(contentType);
            Debug.Print(json);
        }

        private static IMediaType BuildMediaType()
        {
            var builder = new MediaTypeBuilder();
            return builder
                .WithId(10)
                .WithAlias(Constants.Conventions.MediaTypes.Image)
                .WithName("Image")
                .WithCreatorId(22)
                .WithDescription("test")
                .WithIcon("icon")
                .WithThumbnail("thumb")
                .WithSortOrder(5)
                .WithLevel(3)
                .WithPath("-1,4,10")
                .WithPropertyTypeIdsIncrementingFrom(200)
                .WithMediaPropertyGroup()
                .Build();
        }

        [Test]
        public void Can_Deep_Clone_Member_Type()
        {
            // Arrange
            var contentType = BuildMemberType();

            // Act
            var clone = (MemberType)contentType.DeepClone();

            // Assert
            Assert.AreNotSame(clone, contentType);
            Assert.AreEqual(clone, contentType);
            Assert.AreEqual(clone.Id, contentType.Id);
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
            Assert.AreEqual(clone.MemberTypePropertyTypes, ((MemberType)contentType).MemberTypePropertyTypes);

            // This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
        }

        [Test]
        public void Can_Serialize_Member_Type_Without_Error()
        {
            // Arrange
            var contentType = BuildMemberType();

            var json = JsonConvert.SerializeObject(contentType);
            Debug.Print(json);
        }

        private static IMemberType BuildMemberType()
        {
            var builder = new MemberTypeBuilder();
            return builder
                .WithId(10)
                .WithAlias("memberType")
                .WithName("Member type")
                .WithCreatorId(22)
                .WithDescription("test")
                .WithIcon("icon")
                .WithThumbnail("thumb")
                .WithSortOrder(5)
                .WithLevel(3)
                .WithPath("-1,4,10")
                .WithPropertyTypeIdsIncrementingFrom(200)
                .AddPropertyGroup()
                    .WithName("Content")
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Nvarchar)
                        .WithAlias("title")
                        .WithName("Title")
                        .WithSortOrder(1)
                        .WithDataTypeId(-88)
                        .Done()
                    .AddPropertyType()
                        .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
                        .WithValueStorageType(ValueStorageType.Ntext)
                        .WithAlias("bodyText")
                        .WithName("Body text")
                        .WithSortOrder(2)
                        .WithDataTypeId(-87)
                        .Done()
                    .Done()
                .WithMemberCanEditProperty("title", true)
                .WithMemberCanViewProperty("bodyText", true)
                .Build();
        }
    }
}
