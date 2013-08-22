using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Models
{
    [TestFixture]
    public class ContentTests
    {
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
            var httpPostedFileBase = MockRepository.GenerateMock<HttpPostedFileBase>();
            httpPostedFileBase.Stub(x => x.ContentLength).Return(Convert.ToInt32(stream.Length));
            httpPostedFileBase.Stub(x => x.ContentType).Return("text/plain");
            httpPostedFileBase.Stub(x => x.FileName).Return("sample.txt");
            httpPostedFileBase.Stub(x => x.InputStream).Return(stream);

            // Assert
            content.SetValue("title", httpPostedFileBase);

            // Assert
            Assert.That(content.Properties.Any(), Is.True);
            Assert.That(content.Properties["title"], Is.Not.Null);
            Assert.That(content.Properties["title"].Value, Is.StringContaining("sample.txt"));
        }


        [Test]
        public void Can_Clone_Content()
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
            Assert.That(content.Properties["metaDescription"].Value, Is.EqualTo("This is the meta description for a textpage"));
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
            contentType.PropertyGroups["Content"].PropertyTypes.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                                                        {
                                                                            Alias = "subtitle",
                                                                            Name = "Subtitle",
                                                                            Description = "Optional subtitle",
                                                                            HelpText = "",
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
            var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                   {
                                       Alias = "subtitle", Name = "Subtitle", Description = "Optional subtitle", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88
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
            var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                   {
                                       Alias = "subtitle",
                                       Name = "Subtitle",
                                       Description = "Optional subtitle",
                                       HelpText = "",
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
            var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                   {
                                       Alias = "title", Name = "Title", Description = "Title description added", HelpText = "", Mandatory = false, SortOrder = 10, DataTypeDefinitionId = -88
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
            Assert.That(content.Properties.Contains("metaDescription"), Is.True);
            Assert.That(content.Properties["keywords"].Value, Is.EqualTo("text,page,meta"));
            Assert.That(content.Properties["metaDescription"].Value, Is.EqualTo("This is the meta description for a textpage"));
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
            Assert.That(content.Properties.Contains("metaDescription"), Is.False);
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
            var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                   {
                                       Alias = "subtitle",
                                       Name = "Subtitle",
                                       Description = "Optional subtitle",
                                       HelpText = "",
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
                                                                                            new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                                                                                {
                                                                                                    Alias = "coauthor",
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    HelpText = "",
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
                                                                                            new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                                                                                {
                                                                                                    Alias = "coauthor",
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    HelpText = "",
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
            var parent = MockedContentTypes.CreateSimpleContentType("parent", "Parent");
            var meta = MockedContentTypes.CreateMetaContentType();
            var mixin1 = MockedContentTypes.CreateSimpleContentType("mixin1", "Mixin1", new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                                                                                {
                                                                                                    Alias = "coauthor",
                                                                                                    Name = "Co-Author",
                                                                                                    Description = "Name of the Co-Author",
                                                                                                    HelpText = "",
                                                                                                    Mandatory = false,
                                                                                                    SortOrder = 4,
                                                                                                    DataTypeDefinitionId = -88
                                                                                                }
                                                                                        }));
            var mixin2 = MockedContentTypes.CreateSimpleContentType("mixin2", "Mixin2", new PropertyTypeCollection(
                                                                                    new List<PropertyType>
                                                                                        {
                                                                                            new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                                                                                                {
                                                                                                    Alias = "author",
                                                                                                    Name = "Author",
                                                                                                    Description = "Name of the Author",
                                                                                                    HelpText = "",
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