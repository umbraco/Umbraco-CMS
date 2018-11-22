using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

using Umbraco.Core.Serialization;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTests : BaseUmbracoApplicationTest
    {
        [SetUp]
        public void Init()
        {
            var config = SettingsForTests.GetDefault();
            SettingsForTests.ConfigureSettings(config);
        }

        [TearDown]
        public void Dispose()
        {
            
        }

        [TestCase("-1,-20,12,34,56", false)]
        [TestCase("-1,-21,12,34,56", true)]
        [TestCase("-1,12,34,56", false)]
        public void Is_Media_In_Recycle_Bin(string path, bool isInBin)
        {
            var mediaType = MockedContentTypes.CreateImageMediaType();
            var media = MockedMedia.CreateMediaFile(mediaType, -1);
            media.Path = path;
            media.Id = 34;

            Assert.AreEqual(isInBin, media.IsInRecycleBin());
        }

        [TestCase("-1,-20,12,34,56", true)]
        [TestCase("-1,-21,12,34,56", false)]
        [TestCase("-1,12,34,56", false)]
        public void Is_Content_In_Recycle_Bin(string path, bool isInBin)
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateSimpleContent(contentType);
            content.Path = path;
            content.Id = 34;

            Assert.AreEqual(isInBin, content.IsInRecycleBin());
        }

        [Test]
        public void Get_Non_Grouped_Properties()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            //add non-grouped properties
            contentType.AddPropertyType(new PropertyType("test", DataTypeDatabaseType.Ntext, "nonGrouped1") { Name = "Non Grouped 1", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentType.AddPropertyType(new PropertyType("test", DataTypeDatabaseType.Ntext, "nonGrouped2") { Name = "Non Grouped 2", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            
            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);


            var content = MockedContent.CreateSimpleContent(contentType);
            //need to id the p

            var nonGrouped = content.GetNonGroupedProperties();

            Assert.AreEqual(2, nonGrouped.Count());
            Assert.AreEqual(5, content.Properties.Count());
        }

        [Test]
        public void All_Dirty_Properties_Get_Reset()
        {
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act

            // Assert
            Assert.That(content, Is.Not.Null);
        }

        [Test]
        public void Can_Change_Property_Value()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.Properties["title"].Value = "This is the new title";

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Value, Is.EqualTo("This is the new title"));
        }

        [Test]
        public void Can_Set_Property_Value_As_String()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.SetValue("title", "This is the new title");

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Value, Is.EqualTo("This is the new title"));
        }

        [Test]
        public void Can_Set_Property_Value_As_HttpPostedFileBase()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            var stream = new MemoryStream(System.Text.Encoding.Default.GetBytes("TestContent"));
            var postedFileMock = new Mock<HttpPostedFileBase>();
            postedFileMock.Setup(x => x.ContentLength).Returns(Convert.ToInt32(stream.Length));
            postedFileMock.Setup(x => x.ContentType).Returns("text/plain");
            postedFileMock.Setup(x => x.FileName).Returns("sample.txt");
            postedFileMock.Setup(x => x.InputStream).Returns(stream);

            // Assert
            content.SetValue("title", postedFileMock.Object);

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Value, Is.StringContaining("sample.txt"));
        }


        [Test]
        public void Can_Clone_Content_With_Reset_Identity()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);
            content.Id = 10;
            content.Key = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");

            // Act
            var clone = content.Clone();

            // Assert
            Assert.AreNotSame(clone, content);
            Assert.AreNotSame(clone.Id, content.Id);
            Assert.AreNotSame(clone.Version, content.Version);
            Assert.That(clone.HasIdentity, Is.False);

            Assert.AreNotSame(content.Properties, clone.Properties);
        }

        [Ignore]
        [Test]
        public void Can_Deep_Clone_Perf_Test()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            content.ExpireDate = DateTime.Now;
            content.Key = Guid.NewGuid();
            content.Language = "en";
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ReleaseDate = DateTime.Now;
            content.ChangePublishedState(PublishedState.Published);
            content.SortOrder = 5;
            content.Template = new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            };
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.Version = Guid.NewGuid();
            content.WriterId = 23;

            ((IUmbracoEntity)content).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)content).AdditionalData.Add("test2", "hello");

            var runtimeCache = new ObjectCacheRuntimeCacheProvider();
            runtimeCache.InsertCacheItem(content.Id.ToString(CultureInfo.InvariantCulture), () => content);

            using (DisposableTimer.DebugDuration<ContentTests>("STARTING PERF TEST WITH RUNTIME CACHE"))
            {
                for (int j = 0; j < 1000; j++)
                {
                    var clone = runtimeCache.GetCacheItem(content.Id.ToString(CultureInfo.InvariantCulture));
                }
            }

            using (DisposableTimer.DebugDuration<ContentTests>("STARTING PERF TEST WITHOUT RUNTIME CACHE"))
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            content.ExpireDate = DateTime.Now;
            content.Key = Guid.NewGuid();
            content.Language = "en";
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ReleaseDate = DateTime.Now;
            content.ChangePublishedState(PublishedState.Published);
            content.SortOrder = 5;
            content.Template = new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            };
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.Version = Guid.NewGuid();
            content.WriterId = 23;

            ((IUmbracoEntity)content).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)content).AdditionalData.Add("test2", "hello");

            // Act
            var clone = (Content)content.DeepClone();

            // Assert
            Assert.AreNotSame(clone, content);
            Assert.AreEqual(clone, content);
            Assert.AreEqual(clone.Id, content.Id);
            Assert.AreEqual(clone.Version, content.Version);
            Assert.AreEqual(((IUmbracoEntity)clone).AdditionalData, ((IUmbracoEntity)content).AdditionalData);
            Assert.AreNotSame(clone.ContentType, content.ContentType);
            Assert.AreEqual(clone.ContentType, content.ContentType);
            Assert.AreEqual(clone.ContentType.PropertyGroups.Count, content.ContentType.PropertyGroups.Count);
            for (var index = 0; index < content.ContentType.PropertyGroups.Count; index++)
            {
                Assert.AreNotSame(clone.ContentType.PropertyGroups[index], content.ContentType.PropertyGroups[index]);
                Assert.AreEqual(clone.ContentType.PropertyGroups[index], content.ContentType.PropertyGroups[index]);
            }
            Assert.AreEqual(clone.ContentType.PropertyTypes.Count(), content.ContentType.PropertyTypes.Count());
            for (var index = 0; index < content.ContentType.PropertyTypes.Count(); index++)
            {
                Assert.AreNotSame(clone.ContentType.PropertyTypes.ElementAt(index), content.ContentType.PropertyTypes.ElementAt(index));
                Assert.AreEqual(clone.ContentType.PropertyTypes.ElementAt(index), content.ContentType.PropertyTypes.ElementAt(index));
            }
            Assert.AreEqual(clone.ContentTypeId, content.ContentTypeId);
            Assert.AreEqual(clone.CreateDate, content.CreateDate);
            Assert.AreEqual(clone.CreatorId, content.CreatorId);
            Assert.AreEqual(clone.ExpireDate, content.ExpireDate);
            Assert.AreEqual(clone.Key, content.Key);
            Assert.AreEqual(clone.Language, content.Language);
            Assert.AreEqual(clone.Level, content.Level);
            Assert.AreEqual(clone.Path, content.Path);
            Assert.AreEqual(clone.ReleaseDate, content.ReleaseDate);
            Assert.AreEqual(clone.Published, content.Published);
            Assert.AreEqual(clone.PublishedState, content.PublishedState);
            Assert.AreEqual(clone.SortOrder, content.SortOrder);
            Assert.AreEqual(clone.PublishedState, content.PublishedState);
            Assert.AreNotSame(clone.Template, content.Template);
            Assert.AreEqual(clone.Template, content.Template);
            Assert.AreEqual(clone.Trashed, content.Trashed);
            Assert.AreEqual(clone.UpdateDate, content.UpdateDate);
            Assert.AreEqual(clone.Version, content.Version);
            Assert.AreEqual(clone.WriterId, content.WriterId);
            Assert.AreNotSame(clone.Properties, content.Properties);
            Assert.AreEqual(clone.Properties.Count(), content.Properties.Count());
            for (var index = 0; index < content.Properties.Count; index++)
            {
                Assert.AreNotSame(clone.Properties[index], content.Properties[index]);
                Assert.AreEqual(clone.Properties[index], content.Properties[index]);
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
            clone.Properties.Add(new Property(1, Guid.NewGuid(), new PropertyType("test", DataTypeDatabaseType.Ntext, "blah"), "blah"));
            Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));
        }

        [Test]
        public void Can_Serialize_Without_Error()
        {
            var ss = new SerializationService(new JsonNetSerializer());

            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            content.ExpireDate = DateTime.Now;
            content.Key = Guid.NewGuid();
            content.Language = "en";
            content.Level = 3;
            content.Path = "-1,4,10";
            content.ReleaseDate = DateTime.Now;
            content.ChangePublishedState(PublishedState.Published);
            content.SortOrder = 5;
            content.Template = new Template("-1,2,3,4", "Test Template", "testTemplate")
            {
                Id = 88
            };
            content.Trashed = false;
            content.UpdateDate = DateTime.Now;
            content.Version = Guid.NewGuid();
            content.WriterId = 23;

            ((IUmbracoEntity)content).AdditionalData.Add("test1", 123);
            ((IUmbracoEntity)content).AdditionalData.Add("test2", "hello");

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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.PropertyValues(new {title = "This is the new title"});

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Alias, Is.EqualTo("title"));
            Assert.That(content.Properties["title"].Value, Is.EqualTo("This is the new title"));
            Assert.That(content.Properties["description"].Value, Is.EqualTo("This is the meta description for a textpage"));
        }

        [Test]
        public void Can_Verify_Dirty_Property_On_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            contentType.PropertyGroups.Add(new PropertyGroup{ Name = "Test Group", SortOrder = 3 });

            // Assert
            Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(3));
            Assert.That(content.PropertyGroups.Count(), Is.EqualTo(3));
        }

        [Test]
        public void Can_Remove_PropertyGroup_From_ContentType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                                                                        {
                                                                            Name = "Subtitle",
                                                                            Description = "Optional subtitle",
                                                                            Mandatory = false,
                                                                            SortOrder = 3,
                                                                            DataTypeDefinitionId = -88
                                                                        });

            // Assert
            Assert.That(contentType.PropertyGroups["Content"].PropertyTypes.Count, Is.EqualTo(3));
            Assert.That(content.PropertyGroups.First(x => x.Name == "Content").PropertyTypes.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_Add_New_Property_To_New_PropertyType()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                                   {
                                        Name = "Subtitle", Description = "Optional subtitle", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88
                                   };
            contentType.PropertyGroups["Content"].PropertyTypes.Add(propertyType);
            content.Properties.Add(new Property(propertyType){Value = "This is a subtitle Test"});

            // Assert
            Assert.That(content.Properties.Contains("subtitle"), Is.True);
            Assert.That(content.Properties["subtitle"].Value, Is.EqualTo("This is a subtitle Test"));
        }

        [Test]
        public void Can_Add_New_Property_To_New_PropertyType_In_New_PropertyGroup()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                                   {
                                       Name = "Subtitle",
                                       Description = "Optional subtitle",
                                       Mandatory = false,
                                       SortOrder = 3,
                                       DataTypeDefinitionId = -88
                                   };
            var propertyGroup = new PropertyGroup {Name = "Test Group", SortOrder = 3};
            propertyGroup.PropertyTypes.Add(propertyType);
            contentType.PropertyGroups.Add(propertyGroup);
            content.Properties.Add(new Property(propertyType){ Value = "Subtitle Test"});

            // Assert
            Assert.That(content.Properties.Count, Is.EqualTo(5));
            Assert.That(content.PropertyTypes.Count(), Is.EqualTo(5));
            Assert.That(content.PropertyGroups.Count(), Is.EqualTo(3));
            Assert.That(content.Properties["subtitle"].Value, Is.EqualTo("Subtitle Test"));
            Assert.That(content.Properties["title"].Value, Is.EqualTo("Textpage textpage"));
        }

        [Test]
        public void Can_Update_PropertyType_Through_Content_Properties()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act - note that the PropertyType's properties like SortOrder is not updated through the Content object
            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "title")
                                   {
                                        Name = "Title", Description = "Title description added", Mandatory = false, SortOrder = 10, DataTypeDefinitionId = -88
                                   };
            content.Properties.Add(new Property(propertyType));

            // Assert
            Assert.That(content.Properties.Count, Is.EqualTo(4));
            Assert.That(contentType.PropertyTypes.First(x => x.Alias == "title").SortOrder, Is.EqualTo(1));
            Assert.That(content.Properties["title"].Value, Is.EqualTo("Textpage textpage"));
        }

        [Test]
        public void Can_Change_ContentType_On_Content()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.PropertyGroups.Count(), Is.EqualTo(1));
            Assert.That(content.PropertyTypes.Count(), Is.EqualTo(3));
            //Note: There was 4 properties, after changing ContentType 1 has been added (no properties are deleted)
            Assert.That(content.Properties.Count, Is.EqualTo(5));
        }

        [Test]
        public void Can_Change_ContentType_On_Content_And_Set_Property_Value()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);
            content.SetValue("author", "John Doe");

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.Properties["author"].Value, Is.EqualTo("John Doe"));
        }

        [Test]
        public void Can_Change_ContentType_On_Content_And_Still_Get_Old_Properties()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType);

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.Properties.Contains("keywords"), Is.True);
            Assert.That(content.Properties.Contains("description"), Is.True);
            Assert.That(content.Properties["keywords"].Value, Is.EqualTo("text,page,meta"));
            Assert.That(content.Properties["description"].Value, Is.EqualTo("This is the meta description for a textpage"));
        }

        [Test]
        public void Can_Change_ContentType_On_Content_And_Clear_Old_PropertyTypes()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var simpleContentType = MockedContentTypes.CreateSimpleContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ChangeContentType(simpleContentType, true);

            // Assert
            Assert.That(content.Properties.Contains("author"), Is.True);
            Assert.That(content.Properties.Contains("keywords"), Is.False);
            Assert.That(content.Properties.Contains("description"), Is.False);
        }

        [Test]
        public void Can_Verify_Content_Is_Published()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ResetDirtyProperties();
            content.ChangePublishedState(PublishedState.Published);

            // Assert
            Assert.That(content.IsPropertyDirty("Published"), Is.True);
            Assert.That(content.Published, Is.True);
            Assert.That(content.IsPropertyDirty("Name"), Is.False);
        }

        [Test]
        public void Can_Verify_Content_Is_Trashed()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            var content = MockedContent.CreateTextpageContent(contentType, "Textpage", -1);

            // Act
            content.ResetDirtyProperties();
            content.ChangeTrashedState(true);

            // Assert
            Assert.That(content.IsPropertyDirty("Trashed"), Is.True);
            Assert.That(content.Trashed, Is.True);
            Assert.That(content.IsPropertyDirty("Name"), Is.False);
        }

        [Test]
        public void Adding_PropertyGroup_To_ContentType_Results_In_Dirty_Entity()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.ResetDirtyProperties();

            // Act
            var propertyGroup = new PropertyGroup { Name = "Test Group", SortOrder = 3 };
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.ResetDirtyProperties(); //reset
            var content = MockedContent.CreateTextpageContent(contentType, "test", -1);
            content.ResetDirtyProperties();

            // Act
            content.SetPropertyValue("title", "new title");
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
            var contentType = MockedContentTypes.CreateTextpageContentType();

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
            var contentType = MockedContentTypes.CreateTextpageContentType();
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
            var contentType = MockedContentTypes.CreateTextpageContentType();
            contentType.ResetDirtyProperties();

            // Act
            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "subtitle")
                                   {
                                       Name = "Subtitle",
                                       Description = "Optional subtitle",
                                       Mandatory = false,
                                       SortOrder = 3,
                                       DataTypeDefinitionId = -88
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
                                                                                new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", DataTypeDatabaseType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeDefinitionId = -88
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
                                                                                new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", DataTypeDatabaseType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeDefinitionId = -88
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
            var textPage = MockedContentTypes.CreateTextpageContentType();
            var parent = MockedContentTypes.CreateSimpleContentType("parent", "Parent", null, true);
            var meta = MockedContentTypes.CreateMetaContentType();
            var mixin1 = MockedContentTypes.CreateSimpleContentType("mixin1", "Mixin1", new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", DataTypeDatabaseType.Ntext, "coauthor")
                                                                                                {
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeDefinitionId = -88
                                                                                                }
                                                                                        }));
            var mixin2 = MockedContentTypes.CreateSimpleContentType("mixin2", "Mixin2", new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType("test", DataTypeDatabaseType.Ntext, "author")
                                                                                                {
                                                                                                    Name = "Author",
                                                                                                    Description = "Name of the Author",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeDefinitionId = -88
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