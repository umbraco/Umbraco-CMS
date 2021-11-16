﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Moq;
using Umbraco.Core;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTests : UmbracoTestBase
    {
        private IContentTypeService _contentTypeService;

        protected override void Compose()
        {
            base.Compose();

            Composition.Register(_ => Mock.Of<ILogger>());
            Composition.ComposeFileSystems();

            Composition.Register(_ => Mock.Of<IDataTypeService>());
            Composition.Register(_ => Mock.Of<IContentSection>());

            // all this is required so we can validate properties...
            var editor = new TextboxPropertyEditor(Mock.Of<ILogger>()) { Alias = "test" };
            Composition.Register(_ => new DataEditorCollection(new[] { editor }));
            Composition.Register<PropertyEditorCollection>();
            var dataType = Mock.Of<IDataType>();
            Mock.Get(dataType).Setup(x => x.Configuration).Returns(() => new object());
            var dataTypeService = Mock.Of<IDataTypeService>();
            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(() => dataType);

            _contentTypeService = Mock.Of<IContentTypeService>();
            var mediaTypeService = Mock.Of<IMediaTypeService>();
            var memberTypeService = Mock.Of<IMemberTypeService>();
            Composition.Register(_ => ServiceContext.CreatePartial(dataTypeService: dataTypeService, contentTypeBaseServiceProvider: new ContentTypeBaseServiceProvider(_contentTypeService, mediaTypeService, memberTypeService)));

        }

        [Test]
        public void Variant_Culture_Names_Track_Dirty_Changes()
        {
            var contentType = new ContentType(-1) { Alias = "contentType" };
            contentType.Variations = ContentVariation.Culture;
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            const string langFr = "fr-FR";

            Assert.IsFalse(content.IsPropertyDirty("CultureInfos"));    //hasn't been changed

            Thread.Sleep(500);                                          //The "Date" wont be dirty if the test runs too fast since it will be the same date
            content.SetCultureName("name-fr", langFr);
            Assert.IsTrue(content.IsPropertyDirty("CultureInfos"));     //now it will be changed since the collection has changed
            var frCultureName = content.CultureInfos[langFr];
            Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));

            content.ResetDirtyProperties();

            Assert.IsFalse(content.IsPropertyDirty("CultureInfos"));    //it's been reset
            Assert.IsTrue(content.WasPropertyDirty("CultureInfos"));

            Thread.Sleep(500);                                          //The "Date" wont be dirty if the test runs too fast since it will be the same date
            content.SetCultureName("name-fr", langFr);
            Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));
            Assert.IsTrue(content.IsPropertyDirty("CultureInfos"));     //it's true now since we've updated a name
        }

        [Test]
        public void Variant_Published_Culture_Names_Track_Dirty_Changes()
        {
            var contentType = new ContentType(-1) { Alias = "contentType" };
            var content = new Content("content", -1, contentType) { Id = 1, VersionId = 1 };

            const string langFr = "fr-FR";

            contentType.Variations = ContentVariation.Culture;

            content.ChangeContentType(contentType);

            Assert.IsFalse(content.IsPropertyDirty("PublishCultureInfos"));    //hasn't been changed

            Thread.Sleep(500);                                          //The "Date" wont be dirty if the test runs too fast since it will be the same date
            content.SetCultureName("name-fr", langFr);
            content.PublishCulture(CultureImpact.Explicit(langFr, false));                             //we've set the name, now we're publishing it
            Assert.IsTrue(content.IsPropertyDirty("PublishCultureInfos"));     //now it will be changed since the collection has changed
            var frCultureName = content.PublishCultureInfos[langFr];
            Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));

            content.ResetDirtyProperties();

            Assert.IsFalse(content.IsPropertyDirty("PublishCultureInfos"));    //it's been reset
            Assert.IsTrue(content.WasPropertyDirty("PublishCultureInfos"));

            Thread.Sleep(500);                                          //The "Date" wont be dirty if the test runs too fast since it will be the same date
            content.SetCultureName("name-fr", langFr);
            content.PublishCulture(CultureImpact.Explicit(langFr, false));                             //we've set the name, now we're publishing it
            Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));
            Assert.IsTrue(content.IsPropertyDirty("PublishCultureInfos"));     //it's true now since we've updated a name
        }

        [Test]
        public void Get_Non_Grouped_Properties()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            //add non-grouped properties
            contentType.AddPropertyType(new PropertyType("test", ValueStorageType.Ntext, "nonGrouped1") { Name = "Non Grouped 1", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentType.AddPropertyType(new PropertyType("test", ValueStorageType.Ntext, "nonGrouped2") { Name = "Non Grouped 2", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });

            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);


            var content = MockedContent.CreateSimpleContent(contentType);
            //need to id the p

            var nonGrouped = content.GetNonGroupedProperties();

            Assert.AreEqual(2, nonGrouped.Count());
            Assert.AreEqual(5, content.Properties.Count());
        }

        [Test]
        public void All_Dirty_Properties_Get_Reset()
        {
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties(false);

            Assert.IsFalse(content.IsDirty());
            foreach (var prop in content.Properties)
            {
                Assert.IsFalse(prop.IsDirty());
            }
        }

        [Test]
        public void Can_Verify_Mocked_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act

            // Assert
            Assert.That(content, Is.Not.Null);
        }

        [Test]
        public void Can_Change_Property_Value()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.Properties["title"].SetValue("This is the new title");

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
        }

        [Test]
        public void Can_Set_Property_Value_As_String()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.SetValue("title", "This is the new title");

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
        }

        [Test]
        public void Can_Clone_Content_With_Reset_Identity()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            content.Id = 10;
            content.Key = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");

            // Act
            var clone = content.DeepCloneWithResetIdentities();

            // Assert
            Assert.AreNotSame(clone, content);
            Assert.AreNotSame(clone.Id, content.Id);
            Assert.AreNotSame(clone.VersionId, content.VersionId);
            Assert.That(clone.HasIdentity, Is.False);

            Assert.AreNotSame(content.Properties, clone.Properties);
        }

        private static IProfilingLogger GetTestProfilingLogger()
        {
            var logger = new DebugDiagnosticsLogger();
            var profiler = new TestProfiler();
            return new ProfilingLogger(logger, profiler);
        }

        [Ignore("fixme - ignored test")]
        [Test]
        public void Can_Deep_Clone_Perf_Test()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            var i = 200;
            foreach (var property in content.Properties)
            {
                property.Id = ++i;
            }
            content.Id = 10;
            content.CreateDate = DateTime.Now;
            content.CreatorId = 22;
            content.Key = Guid.NewGuid();
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ContentSchedule.Add(DateTime.Now, DateTime.Now.AddDays(1));
            //content.ChangePublishedState(PublishedState.Published);
            content.SortOrder = 5;
            content.TemplateId = 88;
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.WriterId = 23;

            var runtimeCache = new ObjectCacheAppCache();
            runtimeCache.Insert(content.Id.ToString(CultureInfo.InvariantCulture), () => content);

            var proflog = GetTestProfilingLogger();

            using (proflog.DebugDuration<ContentTests>("STARTING PERF TEST WITH RUNTIME CACHE"))
            {
                for (int j = 0; j < 1000; j++)
                {
                    var clone = runtimeCache.Get(content.Id.ToString(CultureInfo.InvariantCulture));
                }
            }

            using (proflog.DebugDuration<ContentTests>("STARTING PERF TEST WITHOUT RUNTIME CACHE"))
            {
                for (int j = 0; j < 1000; j++)
                {
                    var clone = (ContentType)contentType.DeepClone();
                }
            }
        }

        [Test]
        public void Can_Deep_Clone()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;
            contentType.Variations = ContentVariation.Culture;
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.SetCultureName("Hello", "en-US");
            content.SetCultureName("World", "es-ES");
            content.PublishCulture(CultureImpact.All);

            // should not try to clone something that's not Published or Unpublished
            // (and in fact it will not work)
            // but we cannot directly set the state to Published - hence this trick
            //content.ChangePublishedState(PublishedState.Publishing);
            content.ResetDirtyProperties(false); // => .Published

            var i = 200;
            foreach (var property in content.Properties)
            {
                property.Id = ++i;
            }
            content.Id = 10;
            content.CreateDate = DateTime.Now;
            content.CreatorId = 22;
            content.Key = Guid.NewGuid();
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ContentSchedule.Add(DateTime.Now, DateTime.Now.AddDays(1));
            content.SortOrder = 5;
            content.TemplateId = 88;
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.WriterId = 23;



            // Act
            var clone = (Content)content.DeepClone();

            // Assert
            Assert.AreNotSame(clone, content);
            Assert.AreEqual(clone, content);
            Assert.AreEqual(clone.Id, content.Id);
            Assert.AreEqual(clone.VersionId, content.VersionId);
            Assert.AreEqual(clone.ContentType, content.ContentType);
            Assert.AreEqual(clone.ContentTypeId, content.ContentTypeId);
            Assert.AreEqual(clone.CreateDate, content.CreateDate);
            Assert.AreEqual(clone.CreatorId, content.CreatorId);
            Assert.AreEqual(clone.Key, content.Key);
            Assert.AreEqual(clone.Level, content.Level);
            Assert.AreEqual(clone.Path, content.Path);
            Assert.IsTrue(clone.ContentSchedule.Equals(content.ContentSchedule));
            Assert.AreEqual(clone.Published, content.Published);
            Assert.AreEqual(clone.PublishedState, content.PublishedState);
            Assert.AreEqual(clone.SortOrder, content.SortOrder);
            Assert.AreEqual(clone.PublishedState, content.PublishedState);
            Assert.AreNotSame(clone.TemplateId, content.TemplateId);
            Assert.AreEqual(clone.TemplateId, content.TemplateId);
            Assert.AreEqual(clone.Trashed, content.Trashed);
            Assert.AreEqual(clone.UpdateDate, content.UpdateDate);
            Assert.AreEqual(clone.VersionId, content.VersionId);
            Assert.AreEqual(clone.WriterId, content.WriterId);
            Assert.AreNotSame(clone.Properties, content.Properties);
            Assert.AreEqual(clone.Properties.Count(), content.Properties.Count());
            for (var index = 0; index < content.Properties.Count; index++)
            {
                Assert.AreNotSame(clone.Properties[index], content.Properties[index]);
                Assert.AreEqual(clone.Properties[index], content.Properties[index]);
            }

            Assert.AreNotSame(clone.PublishCultureInfos, content.PublishCultureInfos);
            Assert.AreEqual(clone.PublishCultureInfos.Count, content.PublishCultureInfos.Count);
            foreach (var key in content.PublishCultureInfos.Keys)
            {
                Assert.AreNotSame(clone.PublishCultureInfos[key], content.PublishCultureInfos[key]);
                Assert.AreEqual(clone.PublishCultureInfos[key], content.PublishCultureInfos[key]);
            }

            Assert.AreNotSame(clone.CultureInfos, content.CultureInfos);
            Assert.AreEqual(clone.CultureInfos.Count, content.CultureInfos.Count);
            foreach (var key in content.CultureInfos.Keys)
            {
                Assert.AreNotSame(clone.CultureInfos[key], content.CultureInfos[key]);
                Assert.AreEqual(clone.CultureInfos[key], content.CultureInfos[key]);
            }

            //This double verifies by reflection
            var allProps = clone.GetType().GetProperties();
            foreach (var propertyInfo in allProps)
            {
                Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(content, null));
            }

            //need to ensure the event handlers are wired

            var asDirty = (ICanBeDirty)clone;

            Assert.IsFalse(asDirty.IsPropertyDirty("Properties"));
            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "blah");
            var newProperty = new Property(1, propertyType);
            newProperty.SetValue("blah");
            clone.Properties.Add(newProperty);

            Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));
        }

        [Test]
        public void Remember_Dirty_Properties()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;
            contentType.Variations = ContentVariation.Culture;
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.SetCultureName("Hello", "en-US");
            content.SetCultureName("World", "es-ES");
            content.PublishCulture(CultureImpact.All);

            var i = 200;
            foreach (var property in content.Properties)
            {
                property.Id = ++i;
            }
            content.Id = 10;
            content.CreateDate = DateTime.Now;
            content.CreatorId = 22;
            content.ContentSchedule.Add(DateTime.Now, DateTime.Now.AddDays(1));
            content.Key = Guid.NewGuid();
            content.Level = 3;
            content.Path = "-1,4,10";
            content.SortOrder = 5;
            content.TemplateId = 88;

            content.Trashed = true;
            content.UpdateDate = DateTime.Now;
            content.WriterId = 23;

            // Act
            content.ResetDirtyProperties();

            // Assert
            Assert.IsTrue(content.WasDirty());
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Id)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.CreateDate)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.CreatorId)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Key)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Level)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Path)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.ContentSchedule)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.SortOrder)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.TemplateId)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Trashed)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.UpdateDate)));
            Assert.IsTrue(content.WasPropertyDirty(nameof(Content.WriterId)));
            foreach (var prop in content.Properties)
            {
                Assert.IsTrue(prop.WasDirty());
                Assert.IsTrue(prop.WasPropertyDirty("Id"));
            }
            Assert.IsTrue(content.WasPropertyDirty("CultureInfos"));
            foreach (var culture in content.CultureInfos)
            {
                Assert.IsTrue(culture.WasDirty());
                Assert.IsTrue(culture.WasPropertyDirty("Name"));
                Assert.IsTrue(culture.WasPropertyDirty("Date"));
            }
            Assert.IsTrue(content.WasPropertyDirty("PublishCultureInfos"));
            foreach (var culture in content.PublishCultureInfos)
            {
                Assert.IsTrue(culture.WasDirty());
                Assert.IsTrue(culture.WasPropertyDirty("Name"));
                Assert.IsTrue(culture.WasPropertyDirty("Date"));
            }
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.Id = 99;
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            var i = 200;
            foreach (var property in content.Properties)
            {
                property.Id = ++i;
            }
            content.Id = 10;
            content.CreateDate = DateTime.Now;
            content.CreatorId = 22;
            content.Key = Guid.NewGuid();
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ContentSchedule.Add(DateTime.Now, DateTime.Now.AddDays(1));
            //content.ChangePublishedState(PublishedState.Publishing);
            content.SortOrder = 5;
            content.TemplateId = 88;
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.WriterId = 23;

            var result = ss.ToStream(content);
            var json = result.ResultStream.ToJsonString();
            Debug.Print(json);
        }

        /*[Test]
        public void Cannot_Change_Property_With_Invalid_Value()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType);

            // Act
            var model = new TestEditorModel
                            {
                                TestDateTime = DateTime.Now,
                                TestDouble = 1.2,
                                TestInt = 2,
                                TestReadOnly = "Read-only string",
                                TestString = "This is a test string"
                            };

            // Assert
            Assert.Throws<Exception>(() => content.Properties["title"].Value = model);
        }*/

        [Test]
        public void Can_Change_Property_Value_Through_Anonymous_Object()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.PropertyValues(new { title = "This is the new title" });

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Alias, Is.EqualTo("title"));
            Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
            Assert.That(content.Properties["description"].GetValue(), Is.EqualTo("This is the meta description for a textpage"));
        }

        [Test]
        public void Can_Verify_Dirty_Property_On_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ResetDirtyProperties();
            content.Name = "New Home";

            // Assert
            Assert.That(content.Name, Is.EqualTo("New Home"));
            Assert.That(content.IsPropertyDirty("Name"), Is.True);
        }

        [Test]
        public void Can_Add_PropertyGroup_On_ContentType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();

            // Act
            contentType.PropertyGroups.Add(new PropertyGroup(true) { Name = "Test Group", SortOrder = 3 });

            // Assert
            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_Remove_PropertyGroup_From_ContentType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.ResetDirtyProperties();

            // Act
            contentType.PropertyGroups.Remove("Content");

            // Assert
            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(1));
            //Assert.That(contentType.IsPropertyDirty("PropertyGroups"), Is.True);
        }

        [Test]
        public void Can_Add_PropertyType_To_Group_On_ContentType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();

            // Act
            contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType("test", ValueStorageType.Ntext, "subtitle")
            {
                Name = "Subtitle",
                Description = "Optional subtitle",
                Mandatory = false,
                SortOrder = 3,
                DataTypeId = -88
            });

            // Assert
            Assert.That(contentType.PropertyGroups["Content"].PropertyTypes.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_Add_New_Property_To_New_PropertyType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "subtitle")
            {
                Name = "Subtitle",
                Description = "Optional subtitle",
                Mandatory = false,
                SortOrder = 3,
                DataTypeId = -88
            };
            contentType.PropertyGroups["Content"].PropertyTypes.Add(propertyType);
            var newProperty = new Property(propertyType);
            newProperty.SetValue("This is a subtitle Test");
            content.Properties.Add(newProperty);

            // Assert
            Assert.That(content.Properties.Contains("subtitle"), Is.True);
            Assert.That(content.Properties["subtitle"].GetValue(), Is.EqualTo("This is a subtitle Test"));
        }

        [Test]
        public void Can_Add_New_Property_To_New_PropertyType_In_New_PropertyGroup()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "subtitle")
            {
                Name = "Subtitle",
                Description = "Optional subtitle",
                Mandatory = false,
                SortOrder = 3,
                DataTypeId = -88
            };
            var propertyGroup = new PropertyGroup(true) { Name = "Test Group", SortOrder = 3 };
            propertyGroup.PropertyTypes.Add(propertyType);
            contentType.PropertyGroups.Add(propertyGroup);
            var newProperty = new Property(propertyType);
            newProperty.SetValue("Subtitle Test");
            content.Properties.Add(newProperty);

            // Assert
            Assert.That(content.Properties.Count, Is.EqualTo(5));
            Assert.That(content.Properties["subtitle"].GetValue(), Is.EqualTo("Subtitle Test"));
            Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
        }

        [Test]
        public void Can_Update_PropertyType_Through_Content_Properties()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act - note that the PropertyType's properties like SortOrder is not updated through the Content object
            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "title")
            {
                Name = "Title",
                Description = "Title description added",
                Mandatory = false,
                SortOrder = 10,
                DataTypeId = -88
            };
            content.Properties.Add(new Property(propertyType));

            // Assert
            Assert.That(content.Properties.Count, Is.EqualTo(4));
            Assert.That(contentType.PropertyTypes.First(x => x.Alias == "title").SortOrder, Is.EqualTo(1));
            Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
        }

        [Test]
        public void Can_Change_ContentType_On_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            //Note: There was 4 properties, after changing ContentType 1 has been added (no properties are deleted)
            Assert.That(content.Properties.Count, Is.EqualTo(5));
        }

        [Test]
        public void Can_Change_ContentType_On_Content_And_Set_Property_Value()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);
            content.SetValue("author", "John Doe");

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.Properties["author"].GetValue(), Is.EqualTo("John Doe"));
        }

        [Test]
        public void Can_Change_ContentType_On_Content_And_Still_Get_Old_Properties()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.Properties.Contains("keywords"), Is.True);
            Assert.That(content.Properties.Contains("description"), Is.True);
            Assert.That(content.Properties["keywords"].GetValue(), Is.EqualTo("text,page,meta"));
            Assert.That(content.Properties["description"].GetValue(), Is.EqualTo("This is the meta description for a textpage"));
        }

        [Test]
        [Ignore("Need to reimplement this logic for v8")]
        public void Can_Change_ContentType_On_Content_And_Clear_Old_PropertyTypes()
        {
            throw new NotImplementedException();
            //Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            //// Arrange
            //var contentType = MockedContentTypes.CreateTextPageContentType();
            //var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            //var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            //// Act
            //content.ChangeContentType(simpleContentType, true);

            //// Assert
            //Assert.That(content.Properties.Contains("author"), Is.True);
            //Assert.That(content.Properties.Contains("keywords"), Is.False);
            //Assert.That(content.Properties.Contains("description"), Is.False);
        }

        [Test]
        public void Can_Verify_Content_Is_Published()
        {
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            content.ResetDirtyProperties();
            content.PublishedState = PublishedState.Publishing;

            Assert.IsFalse(content.IsPropertyDirty("Published"));
            Assert.IsFalse(content.Published);
            Assert.IsFalse(content.IsPropertyDirty("Name"));
            Assert.AreEqual(PublishedState.Publishing, content.PublishedState);

            // the repo would do
            content.Published = true;

            // and then
            Assert.IsTrue(content.IsPropertyDirty("Published"));
            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.IsPropertyDirty("Name"));
            Assert.AreEqual(PublishedState.Published, content.PublishedState);

            // and before returning,
            content.ResetDirtyProperties();

            // and then
            Assert.IsFalse(content.IsPropertyDirty("Published"));
            Assert.IsTrue(content.Published);
            Assert.IsFalse(content.IsPropertyDirty("Name"));
            Assert.AreEqual(PublishedState.Published, content.PublishedState);
        }

        [Test]
        public void Adding_PropertyGroup_To_ContentType_Results_In_Dirty_Entity()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.ResetDirtyProperties();

            // Act
            var propertyGroup = new PropertyGroup(true) { Name = "Test Group", SortOrder = 3 };
            contentType.PropertyGroups.Add(propertyGroup);

            // Assert
            Assert.That(contentType.IsDirty(), Is.True);
            Assert.That(contentType.PropertyGroups.Any(x => x.Name == "Test Group"), Is.True);
            //Assert.That(contentType.IsPropertyDirty("PropertyGroups"), Is.True);
        }

        [Test]
        public void After_Committing_Changes_Was_Dirty_Is_True()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.ResetDirtyProperties(); //reset

            // Act
            contentType.Alias = "newAlias";
            contentType.ResetDirtyProperties(); //this would be like committing the entity

            // Assert
            Assert.That(contentType.IsDirty(), Is.False);
            Assert.That(contentType.WasDirty(), Is.True);
            Assert.That(contentType.WasPropertyDirty("Alias"), Is.True);
        }

        [Test]
        public void After_Committing_Changes_Was_Dirty_Is_True_On_Changed_Property()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.ResetDirtyProperties(); //reset
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateTextpageContent(contentType, "test", -1);
            content.ResetDirtyProperties();

            // Act
            content.SetValue("title", "new title");
            Assert.That(content.IsEntityDirty(), Is.False);
            Assert.That(content.IsDirty(), Is.True);
            Assert.That(content.IsPropertyDirty("title"), Is.True);
            Assert.That(content.IsAnyUserPropertyDirty(), Is.True);
            Assert.That(content.GetDirtyUserProperties().Count(), Is.EqualTo(1));
            Assert.That(content.Properties[0].IsDirty(), Is.True);
            Assert.That(content.Properties["title"].IsDirty(), Is.True);

            content.ResetDirtyProperties(); //this would be like committing the entity

            // Assert
            Assert.That(content.WasDirty(), Is.True);
            Assert.That(content.Properties[0].WasDirty(), Is.True);


            Assert.That(content.WasPropertyDirty("title"), Is.True);
            Assert.That(content.Properties["title"].IsDirty(), Is.False);
            Assert.That(content.Properties["title"].WasDirty(), Is.True);
        }

        [Test]
        public void If_Not_Committed_Was_Dirty_Is_False()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();

            // Act
            contentType.Alias = "newAlias";

            // Assert
            Assert.That(contentType.IsDirty(), Is.True);
            Assert.That(contentType.WasDirty(), Is.False);
        }

        [Test]
        public void Detect_That_A_Property_Is_Removed()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            Assert.That(contentType.WasPropertyDirty("HasPropertyTypeBeenRemoved"), Is.False);

            // Act
            contentType.RemovePropertyType("title");

            // Assert
            Assert.That(contentType.IsPropertyDirty("HasPropertyTypeBeenRemoved"), Is.True);
        }

        [Test]
        public void Adding_PropertyType_To_PropertyGroup_On_ContentType_Results_In_Dirty_Entity()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextPageContentType();
            contentType.ResetDirtyProperties();

            // Act
            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "subtitle")
            {
                Name = "Subtitle",
                Description = "Optional subtitle",
                Mandatory = false,
                SortOrder = 3,
                DataTypeId = -88
            };
            contentType.PropertyGroups["Content"].PropertyTypes.Add(propertyType);

            // Assert
            Assert.That(contentType.PropertyGroups["Content"].IsDirty(), Is.True);
            Assert.That(contentType.PropertyGroups["Content"].IsPropertyDirty("PropertyTypes"), Is.True);
            Assert.That(contentType.PropertyGroups.Any(x => x.IsDirty()), Is.True);
        }

        [Test]
        public void Can_Compose_Composite_ContentType_Collection()
        {
            // Arrange
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var simple2ContentType = MockedContentTypes.CreateSimpleContentType("anotherSimple", "Another Simple Page",
                                                                                new PropertyTypeCollection(true,
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", ValueStorageType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeId = -88
                                                                                                }
                                                                                        }));

            // Act
            var added = simpleContentType.AddContentType(simple2ContentType);
            var compositionPropertyGroups = simpleContentType.CompositionPropertyGroups;
            var compositionPropertyTypes = simpleContentType.CompositionPropertyTypes;

            // Assert
            Assert.That(added, Is.True);
            Assert.That(compositionPropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(compositionPropertyTypes.Count(), Is.EqualTo(4));
        }

        [Test]
        public void Can_Compose_Nested_Composite_ContentType_Collection()
        {
            // Arrange
            var metaContentType = MockedContentTypes.CreateMetaContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var simple2ContentType = MockedContentTypes.CreateSimpleContentType("anotherSimple", "Another Simple Page",
                                                                                new PropertyTypeCollection(true,
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", ValueStorageType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeId = -88
                                                                                                }
                                                                                        }));

            // Act
            var addedMeta = simple2ContentType.AddContentType(metaContentType);
            var added = simpleContentType.AddContentType(simple2ContentType);
            var compositionPropertyGroups = simpleContentType.CompositionPropertyGroups;
            var compositionPropertyTypes = simpleContentType.CompositionPropertyTypes;

            // Assert
            Assert.That(addedMeta, Is.True);
            Assert.That(added, Is.True);
            Assert.That(compositionPropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(compositionPropertyTypes.Count(), Is.EqualTo(6));
            Assert.That(simpleContentType.ContentTypeCompositionExists("meta"), Is.True);
        }

        [Test]
        public void Can_Avoid_Circular_Dependencies_In_Composition()
        {
            var textPage = MockedContentTypes.CreateTextPageContentType();
            var parent = MockedContentTypes.CreateSimpleContentType("parent", "Parent", null, true);
            var meta = MockedContentTypes.CreateMetaContentType();
            var mixin1 = MockedContentTypes.CreateSimpleContentType("mixin1", "Mixin1", new PropertyTypeCollection(true,
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", ValueStorageType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeId = -88
                                                                                                }
                                                                                        }));
            var mixin2 = MockedContentTypes.CreateSimpleContentType("mixin2", "Mixin2", new PropertyTypeCollection(true,
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", ValueStorageType.Ntext, "author")
                                                                                                {
                                                                                                    Name = "Author",
                                                                                                    Description = "Name of the Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeId = -88
                                                                                                }
                                                                                        }));

            // Act
            var addedMetaMixin2 = mixin2.AddContentType(meta);
            var addedMixin2 = mixin1.AddContentType(mixin2);
            var addedMeta = parent.AddContentType(meta);

            var addedMixin1 = parent.AddContentType(mixin1);

            var addedMixin1Textpage = textPage.AddContentType(mixin1);
            var addedTextpageParent = parent.AddContentType(textPage);

            var aliases = textPage.CompositionAliases();
            var propertyTypes = textPage.CompositionPropertyTypes;
            var propertyGroups = textPage.CompositionPropertyGroups;

            // Assert
            Assert.That(mixin2.ContentTypeCompositionExists("meta"), Is.True);
            Assert.That(mixin1.ContentTypeCompositionExists("meta"), Is.True);
            Assert.That(parent.ContentTypeCompositionExists("meta"), Is.True);
            Assert.That(textPage.ContentTypeCompositionExists("meta"), Is.True);

            Assert.That(aliases.Count(), Is.EqualTo(3));
            Assert.That(propertyTypes.Count(), Is.EqualTo(8));
            Assert.That(propertyGroups.Count(), Is.EqualTo(2));

            Assert.That(addedMeta, Is.True);
            Assert.That(addedMetaMixin2, Is.True);
            Assert.That(addedMixin2, Is.True);
            Assert.That(addedMixin1, Is.False);
            Assert.That(addedMixin1Textpage, Is.True);
            Assert.That(addedTextpageParent, Is.False);
        }
    }
}
