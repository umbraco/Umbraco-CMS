using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(WithApplication = true)]
    public class ContentTypeModelMappingTests : UmbracoTestBase
    {
        // mocks of services that can be setup on a test by test basis to return whatever we want
        private readonly Mock<IContentTypeService> _contentTypeService = new Mock<IContentTypeService>();
        private readonly Mock<IContentService> _contentService = new Mock<IContentService>();
        private readonly Mock<IDataTypeService> _dataTypeService = new Mock<IDataTypeService>();
        private readonly Mock<IEntityService> _entityService = new Mock<IEntityService>();
        private readonly Mock<IFileService> _fileService = new Mock<IFileService>();
        private Mock<PropertyEditorCollection> _editorsMock;

        protected override void Compose()
        {
            base.Compose();

            // create and register a fake property editor collection to return fake property editors
            var editors = new DataEditor[] { new TextboxPropertyEditor(Mock.Of<ILogger>()), };
            var dataEditors = new DataEditorCollection(editors);
            _editorsMock = new Mock<PropertyEditorCollection>(dataEditors);
            _editorsMock.Setup(x => x[It.IsAny<string>()]).Returns(editors[0]);
            Composition.RegisterUnique(f => _editorsMock.Object);

            Composition.RegisterUnique(_ => _contentTypeService.Object);
            Composition.RegisterUnique(_ => _contentService.Object);
            Composition.RegisterUnique(_ => _dataTypeService.Object);
            Composition.RegisterUnique(_ => _entityService.Object);
            Composition.RegisterUnique(_ => _fileService.Object);
        }

        [Test]
        public void MemberTypeSave_To_IMemberType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(Mock.Of<IDataType>(
                    definition =>
                        definition.Id == 555
                        && definition.EditorAlias == "myPropertyType"
                        && definition.DatabaseType == ValueStorageType.Nvarchar));

            var display = CreateMemberTypeSave();

            //Act

            var result = Mapper.Map<IMemberType>(display);

            //Assert

            Assert.AreEqual(display.Alias, result.Alias);
            Assert.AreEqual(display.Description, result.Description);
            Assert.AreEqual(display.Icon, result.Icon);
            Assert.AreEqual(display.Id, result.Id);
            Assert.AreEqual(display.Name, result.Name);
            Assert.AreEqual(display.ParentId, result.ParentId);
            Assert.AreEqual(display.Path, result.Path);
            Assert.AreEqual(display.Thumbnail, result.Thumbnail);
            Assert.AreEqual(display.IsContainer, result.IsContainer);
            Assert.AreEqual(display.AllowAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(display.CreateDate, result.CreateDate);
            Assert.AreEqual(display.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(), result.PropertyGroups.Count);
            for (var i = 0; i < display.Groups.Count(); i++)
            {
                Assert.AreEqual(display.Groups.ElementAt(i).Id, result.PropertyGroups.ElementAt(i).Id);
                Assert.AreEqual(display.Groups.ElementAt(i).Name, result.PropertyGroups.ElementAt(i).Name);
                var propTypes = display.Groups.ElementAt(i).Properties;
                Assert.AreEqual(propTypes.Count(), result.PropertyTypes.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.PropertyTypes.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.PropertyTypes.ElementAt(j).DataTypeId);
                    Assert.AreEqual(propTypes.ElementAt(j).MemberCanViewProperty, result.MemberCanViewProperty(result.PropertyTypes.ElementAt(j).Alias));
                    Assert.AreEqual(propTypes.ElementAt(j).MemberCanEditProperty, result.MemberCanEditProperty(result.PropertyTypes.ElementAt(j).Alias));
                    Assert.AreEqual(propTypes.ElementAt(j).IsSensitiveData, result.IsSensitiveProperty(result.PropertyTypes.ElementAt(j).Alias));
                }
            }

            Assert.AreEqual(display.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < display.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(display.AllowedContentTypes.ElementAt(i), result.AllowedContentTypes.ElementAt(i).Id.Value);
            }
        }

        [Test]
        public void MediaTypeSave_To_IMediaType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(Mock.Of<IDataType>(
                    definition =>
                        definition.Id == 555
                        && definition.EditorAlias == "myPropertyType"
                        && definition.DatabaseType == ValueStorageType.Nvarchar));

            var display = CreateMediaTypeSave();

            //Act

            var result = Mapper.Map<IMediaType>(display);

            //Assert

            Assert.AreEqual(display.Alias, result.Alias);
            Assert.AreEqual(display.Description, result.Description);
            Assert.AreEqual(display.Icon, result.Icon);
            Assert.AreEqual(display.Id, result.Id);
            Assert.AreEqual(display.Name, result.Name);
            Assert.AreEqual(display.ParentId, result.ParentId);
            Assert.AreEqual(display.Path, result.Path);
            Assert.AreEqual(display.Thumbnail, result.Thumbnail);
            Assert.AreEqual(display.IsContainer, result.IsContainer);
            Assert.AreEqual(display.AllowAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(display.CreateDate, result.CreateDate);
            Assert.AreEqual(display.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(), result.PropertyGroups.Count);
            for (var i = 0; i < display.Groups.Count(); i++)
            {
                Assert.AreEqual(display.Groups.ElementAt(i).Id, result.PropertyGroups.ElementAt(i).Id);
                Assert.AreEqual(display.Groups.ElementAt(i).Name, result.PropertyGroups.ElementAt(i).Name);
                var propTypes = display.Groups.ElementAt(i).Properties;
                Assert.AreEqual(propTypes.Count(), result.PropertyTypes.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.PropertyTypes.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.PropertyTypes.ElementAt(j).DataTypeId);
                }
            }

            Assert.AreEqual(display.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < display.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(display.AllowedContentTypes.ElementAt(i), result.AllowedContentTypes.ElementAt(i).Id.Value);
            }
        }

        [Test]
        public void ContentTypeSave_To_IContentType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(Mock.Of<IDataType>(
                    definition =>
                        definition.Id == 555
                        && definition.EditorAlias == "myPropertyType"
                        && definition.DatabaseType == ValueStorageType.Nvarchar));


            _fileService.Setup(x => x.GetTemplate(It.IsAny<string>()))
                .Returns((string alias) => Mock.Of<ITemplate>(
                    definition =>
                        definition.Id == alias.GetHashCode() && definition.Alias == alias));


            var display = CreateContentTypeSave();

            //Act

            var result = Mapper.Map<IContentType>(display);

            //Assert

            Assert.AreEqual(display.Alias, result.Alias);
            Assert.AreEqual(display.Description, result.Description);
            Assert.AreEqual(display.Icon, result.Icon);
            Assert.AreEqual(display.Id, result.Id);
            Assert.AreEqual(display.Name, result.Name);
            Assert.AreEqual(display.ParentId, result.ParentId);
            Assert.AreEqual(display.Path, result.Path);
            Assert.AreEqual(display.Thumbnail, result.Thumbnail);
            Assert.AreEqual(display.IsContainer, result.IsContainer);
            Assert.AreEqual(display.AllowAsRoot, result.AllowedAsRoot);
            Assert.AreEqual(display.CreateDate, result.CreateDate);
            Assert.AreEqual(display.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(), result.PropertyGroups.Count);
            for (var i = 0; i < display.Groups.Count(); i++)
            {
                Assert.AreEqual(display.Groups.ElementAt(i).Id, result.PropertyGroups.ElementAt(i).Id);
                Assert.AreEqual(display.Groups.ElementAt(i).Name, result.PropertyGroups.ElementAt(i).Name);
                var propTypes = display.Groups.ElementAt(i).Properties;
                Assert.AreEqual(propTypes.Count(), result.PropertyTypes.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.PropertyTypes.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.PropertyTypes.ElementAt(j).DataTypeId);
                    Assert.AreEqual(propTypes.ElementAt(j).LabelOnTop, result.PropertyTypes.ElementAt(j).LabelOnTop);
                }
            }

            var allowedTemplateAliases = display.AllowedTemplates
                .Concat(new[] {display.DefaultTemplate})
                .Distinct();

            Assert.AreEqual(allowedTemplateAliases.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < display.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(display.AllowedTemplates.ElementAt(i), result.AllowedTemplates.ElementAt(i).Alias);
            }

            Assert.AreEqual(display.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < display.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(display.AllowedContentTypes.ElementAt(i), result.AllowedContentTypes.ElementAt(i).Id.Value);
            }
        }

        [Test]
        public void MediaTypeSave_With_Composition_To_IMediaType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(Mock.Of<IDataType>(
                    definition =>
                        definition.Id == 555
                        && definition.EditorAlias == "myPropertyType"
                        && definition.DatabaseType == ValueStorageType.Nvarchar));


            var display = CreateCompositionMediaTypeSave();

            //Act

            var result = Mapper.Map<IMediaType>(display);

            //Assert

            // TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(x => x.Inherited == false), result.PropertyGroups.Count);
        }

        [Test]
        public void ContentTypeSave_With_Composition_To_IContentType()
        {
            //Arrange

            // setup the mocks to return the data we want to test against...

            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(Mock.Of<IDataType>(
                    definition =>
                        definition.Id == 555
                        && definition.EditorAlias == "myPropertyType"
                        && definition.DatabaseType == ValueStorageType.Nvarchar));


            var display = CreateCompositionContentTypeSave();

            //Act

            var result = Mapper.Map<IContentType>(display);

            //Assert

            // TODO: Now we need to assert all of the more complicated parts
            Assert.AreEqual(display.Groups.Count(x => x.Inherited == false), result.PropertyGroups.Count);
        }

        [Test]
        public void IMemberType_To_MemberTypeDisplay()
        {
            //Arrange
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            // setup the mocks to return the data we want to test against...

            var memberType = MockedContentTypes.CreateSimpleMemberType();
            memberType.MemberTypePropertyTypes[memberType.PropertyTypes.Last().Alias] = new MemberTypePropertyProfileAccess(true, true, true);

            MockedContentTypes.EnsureAllIds(memberType, 8888);

            //Act

            var result = Mapper.Map<MemberTypeDisplay>(memberType);

            //Assert

            Assert.AreEqual(memberType.Alias, result.Alias);
            Assert.AreEqual(memberType.Description, result.Description);
            Assert.AreEqual(memberType.Icon, result.Icon);
            Assert.AreEqual(memberType.Id, result.Id);
            Assert.AreEqual(memberType.Name, result.Name);
            Assert.AreEqual(memberType.ParentId, result.ParentId);
            Assert.AreEqual(memberType.Path, result.Path);
            Assert.AreEqual(memberType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(memberType.IsContainer, result.IsContainer);
            Assert.AreEqual(memberType.CreateDate, result.CreateDate);
            Assert.AreEqual(memberType.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(memberType.PropertyGroups.Count(), result.Groups.Count());
            for (var i = 0; i < memberType.PropertyGroups.Count(); i++)
            {
                Assert.AreEqual(memberType.PropertyGroups.ElementAt(i).Id, result.Groups.ElementAt(i).Id);
                Assert.AreEqual(memberType.PropertyGroups.ElementAt(i).Name, result.Groups.ElementAt(i).Name);
                var propTypes = memberType.PropertyGroups.ElementAt(i).PropertyTypes;

                Assert.AreEqual(propTypes.Count(), result.Groups.ElementAt(i).Properties.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.Groups.ElementAt(i).Properties.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.Groups.ElementAt(i).Properties.ElementAt(j).DataTypeId);

                    Assert.AreEqual(memberType.MemberCanViewProperty(propTypes.ElementAt(j).Alias), result.Groups.ElementAt(i).Properties.ElementAt(j).MemberCanViewProperty);
                    Assert.AreEqual(memberType.MemberCanEditProperty(propTypes.ElementAt(j).Alias), result.Groups.ElementAt(i).Properties.ElementAt(j).MemberCanEditProperty);
                }
            }

            Assert.AreEqual(memberType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < memberType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(memberType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void IMediaType_To_MediaTypeDisplay()
        {
            //Arrange
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            // setup the mocks to return the data we want to test against...

            var mediaType = MockedContentTypes.CreateImageMediaType();
            MockedContentTypes.EnsureAllIds(mediaType, 8888);

            //Act

            var result = Mapper.Map<MediaTypeDisplay>(mediaType);

            //Assert

            Assert.AreEqual(mediaType.Alias, result.Alias);
            Assert.AreEqual(mediaType.Description, result.Description);
            Assert.AreEqual(mediaType.Icon, result.Icon);
            Assert.AreEqual(mediaType.Id, result.Id);
            Assert.AreEqual(mediaType.Name, result.Name);
            Assert.AreEqual(mediaType.ParentId, result.ParentId);
            Assert.AreEqual(mediaType.Path, result.Path);
            Assert.AreEqual(mediaType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(mediaType.IsContainer, result.IsContainer);
            Assert.AreEqual(mediaType.CreateDate, result.CreateDate);
            Assert.AreEqual(mediaType.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(mediaType.PropertyGroups.Count(), result.Groups.Count());
            for (var i = 0; i < mediaType.PropertyGroups.Count(); i++)
            {
                Assert.AreEqual(mediaType.PropertyGroups.ElementAt(i).Id, result.Groups.ElementAt(i).Id);
                Assert.AreEqual(mediaType.PropertyGroups.ElementAt(i).Name, result.Groups.ElementAt(i).Name);
                var propTypes = mediaType.PropertyGroups.ElementAt(i).PropertyTypes;

                Assert.AreEqual(propTypes.Count(), result.Groups.ElementAt(i).Properties.Count());
                for (var j = 0; j < propTypes.Count(); j++)
                {
                    Assert.AreEqual(propTypes.ElementAt(j).Id, result.Groups.ElementAt(i).Properties.ElementAt(j).Id);
                    Assert.AreEqual(propTypes.ElementAt(j).DataTypeId, result.Groups.ElementAt(i).Properties.ElementAt(j).DataTypeId);
                }
            }

            Assert.AreEqual(mediaType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < mediaType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(mediaType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void IContentType_To_ContentTypeDisplay()
        {
            //Arrange
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            // setup the mocks to return the data we want to test against...

            var contentType = MockedContentTypes.CreateTextPageContentType();
            MockedContentTypes.EnsureAllIds(contentType, 8888);

            //Act

            var result = Mapper.Map<DocumentTypeDisplay>(contentType);

            //Assert

            Assert.AreEqual(contentType.Alias, result.Alias);
            Assert.AreEqual(contentType.Description, result.Description);
            Assert.AreEqual(contentType.Icon, result.Icon);
            Assert.AreEqual(contentType.Id, result.Id);
            Assert.AreEqual(contentType.Name, result.Name);
            Assert.AreEqual(contentType.ParentId, result.ParentId);
            Assert.AreEqual(contentType.Path, result.Path);
            Assert.AreEqual(contentType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(contentType.IsContainer, result.IsContainer);
            Assert.AreEqual(contentType.CreateDate, result.CreateDate);
            Assert.AreEqual(contentType.UpdateDate, result.UpdateDate);
            Assert.AreEqual(contentType.DefaultTemplate.Alias, result.DefaultTemplate.Alias);

            // TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(contentType.PropertyGroups.Count, result.Groups.Count());
            for (var i = 0; i < contentType.PropertyGroups.Count; i++)
            {
                Assert.AreEqual(contentType.PropertyGroups[i].Id, result.Groups.ElementAt(i).Id);
                Assert.AreEqual(contentType.PropertyGroups[i].Name, result.Groups.ElementAt(i).Name);
                var propTypes = contentType.PropertyGroups[i].PropertyTypes;

                Assert.AreEqual(propTypes.Count, result.Groups.ElementAt(i).Properties.Count());
                for (var j = 0; j < propTypes.Count; j++)
                {
                    Assert.AreEqual(propTypes[j].Id, result.Groups.ElementAt(i).Properties.ElementAt(j).Id);
                    Assert.AreEqual(propTypes[j].DataTypeId, result.Groups.ElementAt(i).Properties.ElementAt(j).DataTypeId);
                    Assert.AreEqual(propTypes[j].LabelOnTop, result.Groups.ElementAt(i).Properties.ElementAt(j).LabelOnTop);
                }
            }

            Assert.AreEqual(contentType.AllowedTemplates.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < contentType.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedTemplates.ElementAt(i).Id, result.AllowedTemplates.ElementAt(i).Id);
            }

            Assert.AreEqual(contentType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < contentType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void MemberPropertyGroupBasic_To_MemberPropertyGroup()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new PropertyGroupBasic<MemberPropertyTypeBasic>
            {
                Id = 222,
                Name = "Group 1",
                Alias = "group1",
                SortOrder = 1,
                Properties = new[]
                {
                    new MemberPropertyTypeBasic()
                    {
                        MemberCanEditProperty = true,
                        MemberCanViewProperty = true,
                        IsSensitiveData = true,
                        Id = 33,
                        SortOrder = 1,
                        Alias = "prop1",
                        Description = "property 1",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 1",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = true,
                            Pattern = null
                        }
                    },
                    new MemberPropertyTypeBasic()
                    {
                        MemberCanViewProperty = false,
                        MemberCanEditProperty = false,
                        IsSensitiveData = false,
                        Id = 34,
                        SortOrder = 2,
                        Alias = "prop2",
                        Description = "property 2",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 2",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = false,
                            Pattern = null
                        }
                    },
                }
            };

            var contentType = new MemberTypeSave
            {
                Id = 0,
                ParentId = -1,
                Alias = "alias",
                Groups = new[] { basic }
            };

            // proper group properties mapping takes place when mapping the content type,
            // not when mapping the group - because of inherited properties and such
            //var result = Mapper.Map<PropertyGroup>(basic);
            var result = Mapper.Map<IMemberType>(contentType).PropertyGroups[0];

            Assert.AreEqual(basic.Name, result.Name);
            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Properties.Count(), result.PropertyTypes.Count());
        }

        [Test]
        public void PropertyGroupBasic_To_PropertyGroup()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new PropertyGroupBasic<PropertyTypeBasic>
            {
                Id = 222,
                Name = "Group 1",
                Alias = "group1",
                SortOrder = 1,
                Properties = new[]
                {
                    new PropertyTypeBasic()
                    {
                        Id = 33,
                        SortOrder = 1,
                        Alias = "prop1",
                        Description = "property 1",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 1",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = true,
                            Pattern = null
                        }
                    },
                    new PropertyTypeBasic()
                    {
                        Id = 34,
                        SortOrder = 2,
                        Alias = "prop2",
                        Description = "property 2",
                        DataTypeId = 99,
                        GroupId = 222,
                        Label = "Prop 2",
                        Validation = new PropertyTypeValidation()
                        {
                            Mandatory = false,
                            Pattern = null
                        }
                    },
                }
            };

            var contentType = new DocumentTypeSave
            {
                Id = 0,
                ParentId = -1,
                Alias = "alias",
                AllowedTemplates = Enumerable.Empty<string>(),
                Groups = new[] { basic }
            };

            // proper group properties mapping takes place when mapping the content type,
            // not when mapping the group - because of inherited properties and such
            //var result = Mapper.Map<PropertyGroup>(basic);
            var result = Mapper.Map<IContentType>(contentType).PropertyGroups[0];

            Assert.AreEqual(basic.Name, result.Name);
            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Properties.Count(), result.PropertyTypes.Count());
        }

        [Test]
        public void MemberPropertyTypeBasic_To_PropertyType()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new MemberPropertyTypeBasic()
            {
                Id = 33,
                SortOrder = 1,
                Alias = "prop1",
                Description = "property 1",
                DataTypeId = 99,
                GroupId = 222,
                Label = "Prop 1",
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = true,
                    MandatoryMessage = "Please enter a value",
                    Pattern = "xyz",
                    PatternMessage = "Please match the pattern",
                }
            };

            var result = Mapper.Map<PropertyType>(basic);

            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Alias, result.Alias);
            Assert.AreEqual(basic.Description, result.Description);
            Assert.AreEqual(basic.DataTypeId, result.DataTypeId);
            Assert.AreEqual(basic.Label, result.Name);
            Assert.AreEqual(basic.Validation.Mandatory, result.Mandatory);
            Assert.AreEqual(basic.Validation.MandatoryMessage, result.MandatoryMessage);
            Assert.AreEqual(basic.Validation.Pattern, result.ValidationRegExp);
            Assert.AreEqual(basic.Validation.PatternMessage, result.ValidationRegExpMessage);
            Assert.AreEqual(basic.LabelOnTop, result.LabelOnTop);
        }

        [Test]
        public void PropertyTypeBasic_To_PropertyType()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new PropertyTypeBasic()
            {
                Id = 33,
                SortOrder = 1,
                Alias = "prop1",
                Description = "property 1",
                DataTypeId = 99,
                GroupId = 222,
                Label = "Prop 1",
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = true,
                    MandatoryMessage = "Please enter a value",
                    Pattern = "xyz",
                    PatternMessage = "Please match the pattern",
                }
            };

            var result = Mapper.Map<PropertyType>(basic);

            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Alias, result.Alias);
            Assert.AreEqual(basic.Description, result.Description);
            Assert.AreEqual(basic.DataTypeId, result.DataTypeId);
            Assert.AreEqual(basic.Label, result.Name);
            Assert.AreEqual(basic.Validation.Mandatory, result.Mandatory);
            Assert.AreEqual(basic.Validation.MandatoryMessage, result.MandatoryMessage);
            Assert.AreEqual(basic.Validation.Pattern, result.ValidationRegExp);
            Assert.AreEqual(basic.Validation.PatternMessage, result.ValidationRegExpMessage);
            Assert.AreEqual(basic.LabelOnTop, result.LabelOnTop);
        }

        [Test]
        public void IMediaTypeComposition_To_MediaTypeDisplay()
        {
            //Arrange
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            // setup the mocks to return the data we want to test against...

            _entityService.Setup(x => x.GetObjectType(It.IsAny<int>()))
                .Returns(UmbracoObjectTypes.DocumentType);

            var ctMain = MockedContentTypes.CreateSimpleMediaType("parent", "Parent");
            //not assigned to tab
            ctMain.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "umbracoUrlName",
                Name = "Slug",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            });
            MockedContentTypes.EnsureAllIds(ctMain, 8888);
            var ctChild1 = MockedContentTypes.CreateSimpleMediaType("child1", "Child 1", ctMain, true);
            ctChild1.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "someProperty",
                Name = "Some Property",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            }, "Another tab");
            MockedContentTypes.EnsureAllIds(ctChild1, 7777);
            var contentType = MockedContentTypes.CreateSimpleMediaType("child2", "Child 2", ctChild1, true, "CustomGroup");
            //not assigned to tab
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "umbracoUrlAlias",
                Name = "AltUrl",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            });
            MockedContentTypes.EnsureAllIds(contentType, 6666);


            //Act

            var result = Mapper.Map<MediaTypeDisplay>(contentType);

            //Assert

            Assert.AreEqual(contentType.Alias, result.Alias);
            Assert.AreEqual(contentType.Description, result.Description);
            Assert.AreEqual(contentType.Icon, result.Icon);
            Assert.AreEqual(contentType.Id, result.Id);
            Assert.AreEqual(contentType.Name, result.Name);
            Assert.AreEqual(contentType.ParentId, result.ParentId);
            Assert.AreEqual(contentType.Path, result.Path);
            Assert.AreEqual(contentType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(contentType.IsContainer, result.IsContainer);
            Assert.AreEqual(contentType.CreateDate, result.CreateDate);
            Assert.AreEqual(contentType.UpdateDate, result.UpdateDate);

            // TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(contentType.CompositionPropertyGroups.Select(x => x.Name).Distinct().Count(), result.Groups.Count(x => x.IsGenericProperties == false));
            Assert.AreEqual(1, result.Groups.Count(x => x.IsGenericProperties));
            Assert.AreEqual(contentType.PropertyGroups.Count(), result.Groups.Count(x => x.Inherited == false && x.IsGenericProperties == false));

            var allPropertiesMapped = result.Groups.SelectMany(x => x.Properties).ToArray();
            var allPropertyIdsMapped = allPropertiesMapped.Select(x => x.Id).ToArray();
            var allSourcePropertyIds = contentType.CompositionPropertyTypes.Select(x => x.Id).ToArray();

            Assert.AreEqual(contentType.PropertyTypes.Count(), allPropertiesMapped.Count(x => x.Inherited == false));
            Assert.AreEqual(allPropertyIdsMapped.Count(), allSourcePropertyIds.Count());
            Assert.IsTrue(allPropertyIdsMapped.ContainsAll(allSourcePropertyIds));

            Assert.AreEqual(2, result.Groups.Count(x => x.ParentTabContentTypes.Any()));
            Assert.IsTrue(result.Groups.SelectMany(x => x.ParentTabContentTypes).ContainsAll(new[] { ctMain.Id, ctChild1.Id }));


            Assert.AreEqual(contentType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < contentType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void IContentTypeComposition_To_ContentTypeDisplay()
        {
            //Arrange
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            // setup the mocks to return the data we want to test against...

            _entityService.Setup(x => x.GetObjectType(It.IsAny<int>()))
                .Returns(UmbracoObjectTypes.DocumentType);

            var ctMain = MockedContentTypes.CreateSimpleContentType();
            //not assigned to tab
            ctMain.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "umbracoUrlName", Name = "Slug", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            });
            MockedContentTypes.EnsureAllIds(ctMain, 8888);
            var ctChild1 = MockedContentTypes.CreateSimpleContentType("child1", "Child 1", ctMain, true);
            ctChild1.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "someProperty",
                Name = "Some Property",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            }, "Another tab");
            MockedContentTypes.EnsureAllIds(ctChild1, 7777);
            var contentType = MockedContentTypes.CreateSimpleContentType("child2", "Child 2", ctChild1, true, "CustomGroup");
            //not assigned to tab
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "umbracoUrlAlias", Name = "AltUrl", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            });
            MockedContentTypes.EnsureAllIds(contentType, 6666);


            //Act

            var result = Mapper.Map<DocumentTypeDisplay>(contentType);

            //Assert

            Assert.AreEqual(contentType.Alias, result.Alias);
            Assert.AreEqual(contentType.Description, result.Description);
            Assert.AreEqual(contentType.Icon, result.Icon);
            Assert.AreEqual(contentType.Id, result.Id);
            Assert.AreEqual(contentType.Name, result.Name);
            Assert.AreEqual(contentType.ParentId, result.ParentId);
            Assert.AreEqual(contentType.Path, result.Path);
            Assert.AreEqual(contentType.Thumbnail, result.Thumbnail);
            Assert.AreEqual(contentType.IsContainer, result.IsContainer);
            Assert.AreEqual(contentType.CreateDate, result.CreateDate);
            Assert.AreEqual(contentType.UpdateDate, result.UpdateDate);
            Assert.AreEqual(contentType.DefaultTemplate.Alias, result.DefaultTemplate.Alias);

            // TODO: Now we need to assert all of the more complicated parts

            Assert.AreEqual(contentType.CompositionPropertyGroups.Select(x => x.Name).Distinct().Count(), result.Groups.Count(x => x.IsGenericProperties == false));
            Assert.AreEqual(1, result.Groups.Count(x => x.IsGenericProperties));
            Assert.AreEqual(contentType.PropertyGroups.Count(), result.Groups.Count(x => x.Inherited == false && x.IsGenericProperties == false));

            var allPropertiesMapped = result.Groups.SelectMany(x => x.Properties).ToArray();
            var allPropertyIdsMapped = allPropertiesMapped.Select(x => x.Id).ToArray();
            var allSourcePropertyIds = contentType.CompositionPropertyTypes.Select(x => x.Id).ToArray();

            Assert.AreEqual(contentType.PropertyTypes.Count(), allPropertiesMapped.Count(x => x.Inherited == false));
            Assert.AreEqual(allPropertyIdsMapped.Count(), allSourcePropertyIds.Count());
            Assert.IsTrue(allPropertyIdsMapped.ContainsAll(allSourcePropertyIds));

            Assert.AreEqual(2, result.Groups.Count(x => x.ParentTabContentTypes.Any()));
            Assert.IsTrue(result.Groups.SelectMany(x => x.ParentTabContentTypes).ContainsAll(new[] {ctMain.Id, ctChild1.Id}));

            Assert.AreEqual(contentType.AllowedTemplates.Count(), result.AllowedTemplates.Count());
            for (var i = 0; i < contentType.AllowedTemplates.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedTemplates.ElementAt(i).Id, result.AllowedTemplates.ElementAt(i).Id);
            }

            Assert.AreEqual(contentType.AllowedContentTypes.Count(), result.AllowedContentTypes.Count());
            for (var i = 0; i < contentType.AllowedContentTypes.Count(); i++)
            {
                Assert.AreEqual(contentType.AllowedContentTypes.ElementAt(i).Id.Value, result.AllowedContentTypes.ElementAt(i));
            }

        }

        [Test]
        public void MemberPropertyTypeBasic_To_MemberPropertyTypeDisplay()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new MemberPropertyTypeBasic()
            {
                Id = 33,
                SortOrder = 1,
                Alias = "prop1",
                Description = "property 1",
                DataTypeId = 99,
                GroupId = 222,
                Label = "Prop 1",
                MemberCanViewProperty = true,
                MemberCanEditProperty = true,
                IsSensitiveData = true,
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = true,
                    Pattern = "xyz"
                }
            };

            var result = Mapper.Map<MemberPropertyTypeDisplay>(basic);

            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Alias, result.Alias);
            Assert.AreEqual(basic.Description, result.Description);
            Assert.AreEqual(basic.GroupId, result.GroupId);
            Assert.AreEqual(basic.Inherited, result.Inherited);
            Assert.AreEqual(basic.Label, result.Label);
            Assert.AreEqual(basic.Validation, result.Validation);
            Assert.AreEqual(basic.MemberCanViewProperty, result.MemberCanViewProperty);
            Assert.AreEqual(basic.MemberCanEditProperty, result.MemberCanEditProperty);
            Assert.AreEqual(basic.IsSensitiveData, result.IsSensitiveData);
        }

        [Test]
        public void PropertyTypeBasic_To_PropertyTypeDisplay()
        {
            _dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns(new DataType(new VoidEditor(Mock.Of<ILogger>())));

            var basic = new PropertyTypeBasic()
            {
                Id = 33,
                SortOrder = 1,
                Alias = "prop1",
                Description = "property 1",
                DataTypeId = 99,
                GroupId = 222,
                Label = "Prop 1",
                Validation = new PropertyTypeValidation()
                {
                    Mandatory = true,
                    Pattern = "xyz"
                }
            };

            var result = Mapper.Map<PropertyTypeDisplay>(basic);

            Assert.AreEqual(basic.Id, result.Id);
            Assert.AreEqual(basic.SortOrder, result.SortOrder);
            Assert.AreEqual(basic.Alias, result.Alias);
            Assert.AreEqual(basic.Description, result.Description);
            Assert.AreEqual(basic.GroupId, result.GroupId);
            Assert.AreEqual(basic.Inherited, result.Inherited);
            Assert.AreEqual(basic.Label, result.Label);
            Assert.AreEqual(basic.Validation, result.Validation);
        }

        private MemberTypeSave CreateMemberTypeSave()
        {
            return new MemberTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedContentTypes = new[] { 666, 667 },
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<MemberPropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        Alias = "tab1",
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new[]
                        {
                            new MemberPropertyTypeBasic
                            {
                                MemberCanEditProperty = true,
                                MemberCanViewProperty = true,
                                IsSensitiveData = true,
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555
                            }
                        }
                    }
                }
            };
        }

        private MediaTypeSave CreateMediaTypeSave()
        {
            return new MediaTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedContentTypes = new[] { 666, 667 },
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        Alias = "tab1",
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555
                            }
                        }
                    }
                }
            };
        }

        private DocumentTypeSave CreateContentTypeSave()
        {
            return new DocumentTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedTemplates = new []
                {
                    "template1",
                    "template2"
                },
                AllowedContentTypes = new [] {666, 667},
                DefaultTemplate = "test",
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new []
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        Alias = "tab1",
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                LabelOnTop = true
                            }
                        }
                    }
                }
            };
        }

        private MediaTypeSave CreateCompositionMediaTypeSave()
        {
            return new MediaTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedContentTypes = new[] { 666, 667 },
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        Alias = "tab1",
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                LabelOnTop = true
                            }
                        }
                    },
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 894,
                        Name = "Tab 2",
                        Alias = "tab2",
                        SortOrder = 0,
                        Inherited = true,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "parentProperty",
                                Description = "this is a property from the parent",
                                Inherited = true,
                                Label = "Parent property",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                LabelOnTop = false
                            }
                        }

                    }
                }

            };
        }

        private DocumentTypeSave CreateCompositionContentTypeSave()
        {
            return new DocumentTypeSave
            {
                Alias = "test",
                AllowAsRoot = true,
                AllowedTemplates = new[]
                {
                    "template1",
                    "template2"
                },
                AllowedContentTypes = new[] { 666, 667 },
                DefaultTemplate = "test",
                Description = "hello world",
                Icon = "tree-icon",
                Id = 1234,
                Key = new Guid("8A60656B-3866-46AB-824A-48AE85083070"),
                Name = "My content type",
                Path = "-1,1234",
                ParentId = -1,
                Thumbnail = "tree-thumb",
                IsContainer = true,
                Groups = new[]
                {
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 987,
                        Name = "Tab 1",
                        Alias = "tab1",
                        SortOrder = 0,
                        Inherited = false,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "property1",
                                Description = "this is property 1",
                                Inherited = false,
                                Label = "Property 1",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                LabelOnTop = true
                            }
                        }
                    },
                    new PropertyGroupBasic<PropertyTypeBasic>()
                    {
                        Id = 894,
                        Name = "Tab 2",
                        Alias = "tab2",
                        SortOrder = 0,
                        Inherited = true,
                        Properties = new[]
                        {
                            new PropertyTypeBasic
                            {
                                Alias = "parentProperty",
                                Description = "this is a property from the parent",
                                Inherited = true,
                                Label = "Parent property",
                                Validation = new PropertyTypeValidation
                                {
                                    Mandatory = false,
                                    Pattern = string.Empty
                                },
                                SortOrder = 0,
                                DataTypeId = 555,
                                LabelOnTop = false
                            }
                        }

                    }
                }

            };
        }
    }
}
