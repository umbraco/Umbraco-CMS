using System.Globalization;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing.CompositionExtensions;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Tests.Testing;
using Umbraco.Web.PropertyEditors;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Tests.Models.Mapping
{
    [TestFixture]
    [UmbracoTest(Mapper = true, Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentWebModelMappingTests : TestWithDatabaseBase
    {
        private IContentTypeService _contentTypeService;


        protected override void Compose()
        {
            base.Compose();

            Composition.RegisterUnique(f => Mock.Of<ICultureDictionaryFactory>());

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
            Composition.RegisterUnique(_ => _contentTypeService);
            Composition.Register(_ => ServiceContext.CreatePartial(dataTypeService: dataTypeService, contentTypeBaseServiceProvider: new ContentTypeBaseServiceProvider(_contentTypeService, mediaTypeService, memberTypeService)));
        }

        [DataEditor("Test.Test", "Test", "~/Test.html")]
        public class TestPropertyEditor : DataEditor
        {
            /// <summary>
            /// The constructor will setup the property editor based on the attribute if one is found
            /// </summary>
            public TestPropertyEditor(ILogger logger) : base(logger)
            { }
        }

        private void FixUsers(IContentBase content)
        {
            // CreateSimpleContentType leaves CreatorId == 0
            // which used to be both the "super" user and the "default" user
            // v8 is changing this, so the test would report a <null> creator
            // temp. fixing by assigning super here
            //
            content.CreatorId = Constants.Security.SuperUserId;
        }

        [Test]
        public void To_Media_Item_Simple()
        {
            var contentType = MockedContentTypes.CreateImageMediaType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedMedia.CreateMediaImage(contentType, -1);
            FixUsers(content);

            var result = Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>(content);

            AssertBasics(result, content);

            foreach (var p in content.Properties)
            {
                AssertBasicProperty(result, p);
            }
        }

        [Test]
        public void To_Content_Item_Simple()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);
            FixUsers(content);

            var result = Mapper.Map<IContent, ContentItemBasic<ContentPropertyBasic>>(content);

            AssertBasics(result, content);

            foreach (var p in content.Properties)
            {
                AssertBasicProperty(result, p);
            }
        }

        [Test]
        public void To_Content_Item_Dto()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);
            FixUsers(content);

            var result = Mapper.Map<IContent, ContentPropertyCollectionDto>(content);

            foreach (var p in content.Properties)
            {
                AssertProperty(result, p);
            }
        }

        [Test]
        public void To_Media_Item_Dto()
        {
            var contentType = MockedContentTypes.CreateImageMediaType();
            var content = MockedMedia.CreateMediaImage(contentType, -1);
            FixUsers(content);

            var result = Mapper.Map<IMedia, ContentPropertyCollectionDto>(content);

            foreach (var p in content.Properties)
            {
                AssertProperty(result, p);
            }
        }

        [Test]
        public void To_Display_Model()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);
            Mock.Get(_contentTypeService).Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);


            FixUsers(content);

            // need ids for tabs
            var id = 1;
            foreach (var g in contentType.CompositionPropertyGroups)
                g.Id = id++;

            var result = Mapper.Map<IContent, ContentItemDisplay>(content);

            AssertBasics(result, content);

            var invariantContent = result.Variants.First();
            foreach (var p in content.Properties)
            {
                AssertBasicProperty(invariantContent, p);
                AssertDisplayProperty(invariantContent, p);
            }

            Assert.AreEqual(contentType.CompositionPropertyGroups.Count(), invariantContent.Tabs.Count());
            Assert.IsTrue(invariantContent.Tabs.First().IsActive);
            Assert.IsTrue(invariantContent.Tabs.Except(new[] { invariantContent.Tabs.First() }).All(x => x.IsActive == false));
        }

        [Test]
        public void To_Display_Model_No_Tabs()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType();
            contentType.PropertyGroups.Clear();
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);
            Mock.Get(_contentTypeService).Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);

            var content = new Content("Home", -1, contentType) { Level = 1, SortOrder = 1, CreatorId = 0, WriterId = 0 };

            var result = Mapper.Map<IContent, ContentItemDisplay>(content);

            AssertBasics(result, content);

            var invariantContent = result.Variants.First();
            foreach (var p in content.Properties)
            {
                AssertBasicProperty(invariantContent, p);
                AssertDisplayProperty(invariantContent, p);
            }

            Assert.AreEqual(contentType.CompositionPropertyGroups.Count(), invariantContent.Tabs.Count());
        }

        [Test]
        public void To_Display_Model_With_Non_Grouped_Properties()
        {
            var idSeed = 1;
            var contentType = MockedContentTypes.CreateSimpleContentType();
            //add non-grouped properties
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "nonGrouped1") { Name = "Non Grouped 1", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "nonGrouped2") { Name = "Non Grouped 2", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            //set ids or it wont work
            contentType.Id = idSeed;
            foreach (var p in contentType.PropertyTypes)
            {
                p.Id = idSeed;
                idSeed++;
            }
            Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);
            Mock.Get(_contentTypeService).Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);


            var content = MockedContent.CreateSimpleContent(contentType);
            FixUsers(content);

            foreach (var p in content.Properties)
            {
                p.Id = idSeed;
                idSeed++;
            }
            //need ids for tabs
            var id = 1;
            foreach (var g in contentType.CompositionPropertyGroups)
            {
                g.Id = id;
                id++;
            }
            //ensure that nothing is marked as dirty
            contentType.ResetDirtyProperties(false);
            //ensure that nothing is marked as dirty
            content.ResetDirtyProperties(false);

            var result = Mapper.Map<IContent, ContentItemDisplay>(content);

            AssertBasics(result, content);

            var invariantContent = result.Variants.First();
            foreach (var p in content.Properties)
            {
                AssertBasicProperty(invariantContent, p);
                AssertDisplayProperty(invariantContent, p);
            }

            Assert.AreEqual(contentType.CompositionPropertyGroups.Count(), invariantContent.Tabs.Count() - 1);
            Assert.IsTrue(invariantContent.Tabs.Any(x => x.Label == Current.Services.TextService.Localize("general", "properties")));
            Assert.AreEqual(2, invariantContent.Tabs.Where(x => x.Label == Current.Services.TextService.Localize("general", "properties")).SelectMany(x => x.Properties.Where(p => p.Alias.StartsWith("_umb_") == false)).Count());
        }

        #region Assertions

        private void AssertDisplayProperty<T>(IContentProperties<T> result, Property p)
            where T : ContentPropertyBasic
        {
            var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
            Assert.IsNotNull(pDto);

            //pDto.Alias = p.Alias;
            //pDto.Description = p.PropertyType.Description;
            //pDto.Label = p.PropertyType.Name;
            //pDto.Config = applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(p.PropertyType.DataTypeDefinitionId);

        }

        private void AssertBasics(ContentItemDisplay result, IContent content)
        {
            Assert.AreEqual(content.Id, result.Id);

            var ownerId = content.CreatorId;
            if (ownerId != 0)
            {
                Assert.IsNotNull(result.Owner);
                Assert.AreEqual(Constants.Security.SuperUserId, result.Owner.UserId);
                Assert.AreEqual("Administrator", result.Owner.Name);
            }
            else
            {
                Assert.IsNull(result.Owner); // because, 0 is no user
            }

            var invariantContent = result.Variants.First();

            Assert.AreEqual(content.ParentId, result.ParentId);
            Assert.AreEqual(content.UpdateDate, invariantContent.UpdateDate);
            Assert.AreEqual(content.CreateDate, invariantContent.CreateDate);
            Assert.AreEqual(content.Name, invariantContent.Name);
            Assert.AreEqual(content.Properties.Count(),
                ((IContentProperties<ContentPropertyDisplay>)invariantContent).Properties.Count(x => x.Alias.StartsWith("_umb_") == false));
        }

        private void AssertBasics<T, TPersisted>(ContentItemBasic<T> result, TPersisted content)
            where T : ContentPropertyBasic
            where TPersisted : IContentBase
        {
            Assert.AreEqual(content.Id, result.Id);

            var ownerId = content.CreatorId;
            if (ownerId != 0)
            {
                Assert.IsNotNull(result.Owner);
                Assert.AreEqual(Constants.Security.SuperUserId, result.Owner.UserId);
                Assert.AreEqual("Administrator", result.Owner.Name);
            }
            else
            {
                Assert.IsNull(result.Owner); // because, 0 is no user
            }

            Assert.AreEqual(content.ParentId, result.ParentId);
            Assert.AreEqual(content.UpdateDate, result.UpdateDate);
            Assert.AreEqual(content.CreateDate, result.CreateDate);
            Assert.AreEqual(content.Name, result.Name);
            Assert.AreEqual(content.Properties.Count(), result.Properties.Count(x => x.Alias.StartsWith("_umb_") == false));
        }

        private void AssertBasicProperty<T>(IContentProperties<T> result, Property p)
            where T : ContentPropertyBasic
        {
            var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
            Assert.IsNotNull(pDto);
            Assert.AreEqual(p.Alias, pDto.Alias);
            Assert.AreEqual(p.Id, pDto.Id);

            if (p.GetValue() == null)
                Assert.AreEqual(pDto.Value, string.Empty);
            else if (p.GetValue() is decimal)
                Assert.AreEqual(pDto.Value, ((decimal) p.GetValue()).ToString(NumberFormatInfo.InvariantInfo));
            else
                Assert.AreEqual(pDto.Value, p.GetValue().ToString());
        }

        private void AssertProperty(IContentProperties<ContentPropertyDto> result, Property p)
        {
            AssertBasicProperty(result, p);

            var pDto = result.Properties.SingleOrDefault(x => x.Alias == p.Alias);
            Assert.IsNotNull(pDto);
            Assert.AreEqual(p.PropertyType.Mandatory, pDto.IsRequired);
            Assert.AreEqual(p.PropertyType.ValidationRegExp, pDto.ValidationRegExp);
            Assert.AreEqual(p.PropertyType.Description, pDto.Description);
            Assert.AreEqual(p.PropertyType.Name, pDto.Label);
            Assert.AreEqual(Current.Services.DataTypeService.GetDataType(p.PropertyType.DataTypeId), pDto.DataType);
            Assert.AreEqual(Current.PropertyEditors[p.PropertyType.PropertyEditorAlias], pDto.PropertyEditor);
        }

        private void AssertContentItem<T>(ContentItemBasic<ContentPropertyDto> result, T content)
            where T : IContentBase
        {
            AssertBasics(result, content);

            foreach (var p in content.Properties)
            {
                AssertProperty(result, p);
            }
        }
        #endregion
    }
}
