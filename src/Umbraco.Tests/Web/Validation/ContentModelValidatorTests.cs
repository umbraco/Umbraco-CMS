using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Services;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Editors.Filters;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Editors.Binders;
using Umbraco.Core;
using Umbraco.Tests.Testing;
using Umbraco.Core.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Composing;
using System.Web.Http.ModelBinding;
using Umbraco.Web.PropertyEditors;
using System.ComponentModel.DataAnnotations;
using Umbraco.Tests.TestHelpers;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Web.PropertyEditors.Validation;

namespace Umbraco.Tests.Web.Validation
{
    [UmbracoTest(Mapper = true, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
    [TestFixture]
    public class ContentModelValidatorTests : UmbracoTestBase
    {
        private const int ComplexDataTypeId = 9999;
        private const string ContentTypeAlias = "textPage";
        private ContentType _contentType;

        public override void SetUp()
        {
            base.SetUp();

            _contentType = MockedContentTypes.CreateTextPageContentType(ContentTypeAlias);
            // add complex editor
            _contentType.AddPropertyType(
                new PropertyType("complexTest", ValueStorageType.Ntext) { Alias = "complex", Name = "Complex", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = ComplexDataTypeId },
                "Content");

            // make them all validate with a regex rule that will not pass
            foreach (var prop in _contentType.PropertyTypes)
            {
                prop.ValidationRegExp = "^donotmatch$";
                prop.ValidationRegExpMessage = "Does not match!";
            }
        }

        protected override void Compose()
        {
            base.Compose();

            var complexEditorConfig = new NestedContentConfiguration
            {
                ContentTypes = new[]
                {
                    new NestedContentConfiguration.ContentType { Alias = "feature" }
                }
            };
            var dataTypeService = new Mock<IDataTypeService>();
            dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
                .Returns((int id) => id == ComplexDataTypeId
                        ? Mock.Of<IDataType>(x => x.Configuration == complexEditorConfig)
                        : Mock.Of<IDataType>());

            var contentTypeService = new Mock<IContentTypeService>();
            contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>()))
                .Returns(() => new List<IContentType>
                {
                    _contentType
                });

            var textService = new Mock<ILocalizedTextService>();
            textService.Setup(x => x.Localize("validation/invalidPattern", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns(() => "invalidPattern");
            textService.Setup(x => x.Localize("validation/invalidNull", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns("invalidNull");
            textService.Setup(x => x.Localize("validation/invalidEmpty", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns("invalidEmpty");

            Composition.RegisterUnique(x => Mock.Of<IDataTypeService>(x => x.GetDataType(It.IsAny<int>()) == Mock.Of<IDataType>()));
            Composition.RegisterUnique(x => dataTypeService.Object);
            Composition.RegisterUnique(x => contentTypeService.Object);
            Composition.RegisterUnique(x => textService.Object);

            Composition.WithCollectionBuilder<DataEditorCollectionBuilder>()
                .Add<TestEditor>()
                .Add<ComplexTestEditor>();
        }

        [Test]
        public void Test_Serializer()
        {
            var nestedLevel2 = new ComplexEditorValidationResult();
            var id1 = Guid.NewGuid();
            var addressInfoElementTypeResult = new ComplexEditorElementTypeValidationResult("addressInfo", id1);
            var cityPropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("city");
            cityPropertyTypeResult.AddValidationResult(new ValidationResult("City is invalid"));
            cityPropertyTypeResult.AddValidationResult(new ValidationResult("City cannot be empty"));
            cityPropertyTypeResult.AddValidationResult(new ValidationResult("City is not in Australia", new[] { "country" }));
            cityPropertyTypeResult.AddValidationResult(new ValidationResult("Not a capital city", new[] { "capital" }));
            addressInfoElementTypeResult.ValidationResults.Add(cityPropertyTypeResult);
            nestedLevel2.ValidationResults.Add(addressInfoElementTypeResult);

            var nestedLevel1 = new ComplexEditorValidationResult();
            var id2 = Guid.NewGuid();
            var addressBookElementTypeResult = new ComplexEditorElementTypeValidationResult("addressBook", id2);
            var addressesPropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("addresses");
            addressesPropertyTypeResult.AddValidationResult(new ValidationResult("Must have at least 3 addresses", new[] { "counter" }));
            addressesPropertyTypeResult.AddValidationResult(nestedLevel2); // This is a nested result within the level 1
            addressBookElementTypeResult.ValidationResults.Add(addressesPropertyTypeResult);
            var bookNamePropertyTypeResult = new ComplexEditorPropertyTypeValidationResult("bookName");
            bookNamePropertyTypeResult.AddValidationResult(new ValidationResult("Invalid address book name", new[] { "book" }));
            addressBookElementTypeResult.ValidationResults.Add(bookNamePropertyTypeResult);
            nestedLevel1.ValidationResults.Add(addressBookElementTypeResult);

            var id3 = Guid.NewGuid();
            var addressBookElementTypeResult2 = new ComplexEditorElementTypeValidationResult("addressBook", id3);
            var addressesPropertyTypeResult2 = new ComplexEditorPropertyTypeValidationResult("addresses");
            addressesPropertyTypeResult2.AddValidationResult(new ValidationResult("Must have at least 2 addresses", new[] { "counter" }));
            addressBookElementTypeResult2.ValidationResults.Add(addressesPropertyTypeResult);
            var bookNamePropertyTypeResult2 = new ComplexEditorPropertyTypeValidationResult("bookName");
            bookNamePropertyTypeResult2.AddValidationResult(new ValidationResult("Name is too long"));
            addressBookElementTypeResult2.ValidationResults.Add(bookNamePropertyTypeResult2);
            nestedLevel1.ValidationResults.Add(addressBookElementTypeResult2);

            // books is the outer most validation result and doesn't have it's own direct ValidationResult errors
            var outerError = new ComplexEditorValidationResult();
            var id4 = Guid.NewGuid();
            var addressBookCollectionElementTypeResult = new ComplexEditorElementTypeValidationResult("addressBookCollection", id4);
            var booksPropertyTypeResult= new ComplexEditorPropertyTypeValidationResult("books");
            booksPropertyTypeResult.AddValidationResult(nestedLevel1); // books is the outer most validation result
            addressBookCollectionElementTypeResult.ValidationResults.Add(booksPropertyTypeResult);
            outerError.ValidationResults.Add(addressBookCollectionElementTypeResult);

            var serialized = JsonConvert.SerializeObject(outerError, Formatting.Indented, new ValidationResultConverter());
            Console.WriteLine(serialized);

            var jsonError = JsonConvert.DeserializeObject<JArray>(serialized);

            Assert.IsNotNull(jsonError.SelectToken("$[0]"));
            Assert.AreEqual(id4.ToString(), jsonError.SelectToken("$[0].$id").Value<string>());
            Assert.AreEqual("addressBookCollection", jsonError.SelectToken("$[0].$elementTypeAlias").Value<string>());
            Assert.AreEqual(string.Empty, jsonError.SelectToken("$[0].ModelState['_Properties.books.invariant.null'][0]").Value<string>());

            var error0 = jsonError.SelectToken("$[0].books") as JArray;
            Assert.IsNotNull(error0);
            Assert.AreEqual(id2.ToString(), error0.SelectToken("$[0].$id").Value<string>());
            Assert.AreEqual("addressBook", error0.SelectToken("$[0].$elementTypeAlias").Value<string>());
            Assert.IsNotNull(error0.SelectToken("$[0].ModelState"));
            Assert.AreEqual(string.Empty, error0.SelectToken("$[0].ModelState['_Properties.addresses.invariant.null'][0]").Value<string>());
            var error1 = error0.SelectToken("$[0].ModelState['_Properties.addresses.invariant.null.counter']") as JArray;
            Assert.IsNotNull(error1);
            Assert.AreEqual(1, error1.Count);
            var error2 = error0.SelectToken("$[0].ModelState['_Properties.bookName.invariant.null.book']") as JArray;
            Assert.IsNotNull(error2);
            Assert.AreEqual(1, error2.Count);

            Assert.AreEqual(id3.ToString(), error0.SelectToken("$[1].$id").Value<string>());
            Assert.AreEqual("addressBook", error0.SelectToken("$[1].$elementTypeAlias").Value<string>());
            Assert.IsNotNull(error0.SelectToken("$[1].ModelState"));
            Assert.AreEqual(string.Empty, error0.SelectToken("$[1].ModelState['_Properties.addresses.invariant.null'][0]").Value<string>());
            var error6 = error0.SelectToken("$[1].ModelState['_Properties.addresses.invariant.null.counter']") as JArray;
            Assert.IsNotNull(error6);
            Assert.AreEqual(1, error6.Count);
            var error7 = error0.SelectToken("$[1].ModelState['_Properties.bookName.invariant.null']") as JArray;
            Assert.IsNotNull(error7);
            Assert.AreEqual(1, error7.Count);

            Assert.IsNotNull(error0.SelectToken("$[0].addresses"));
            Assert.AreEqual(id1.ToString(), error0.SelectToken("$[0].addresses[0].$id").Value<string>());
            Assert.AreEqual("addressInfo", error0.SelectToken("$[0].addresses[0].$elementTypeAlias").Value<string>());
            Assert.IsNotNull(error0.SelectToken("$[0].addresses[0].ModelState"));
            var error3 = error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null.country']") as JArray;
            Assert.IsNotNull(error3);
            Assert.AreEqual(1, error3.Count);
            var error4 = error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null.capital']") as JArray;
            Assert.IsNotNull(error4);
            Assert.AreEqual(1, error4.Count);
            var error5 = error0.SelectToken("$[0].addresses[0].ModelState['_Properties.city.invariant.null']") as JArray;
            Assert.IsNotNull(error5);
            Assert.AreEqual(2, error5.Count);
        }

        [Test]
        public void Validating_ContentItemSave()
        {
            var validator = new ContentSaveModelValidator(
                Factory.GetInstance<ILogger>(),
                Mock.Of<IUmbracoContextAccessor>(),
                Factory.GetInstance<ILocalizedTextService>());

            var content = MockedContent.CreateTextpageContent(_contentType, "test", -1);

            var id1 = new Guid("c8df5136-d606-41f0-9134-dea6ae0c2fd9");
            var id2 = new Guid("f916104a-4082-48b2-a515-5c4bf2230f38");
            var id3 = new Guid("77E15DE9-1C79-47B2-BC60-4913BC4D4C6A");

            // TODO: Ok now test with a 4th level complex nested editor

            var complexValue = @"[{
            		""key"": """ + id1.ToString() + @""",
            		""name"": ""Hello world"",
            		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
            		""title"": ""Hello world"",
                    ""bodyText"": ""The world is round""
            	}, {
            		""key"": """ + id2.ToString() + @""",
            		""name"": ""Super nested"",
            		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
            		""title"": ""Hi there!"",
                    ""bodyText"": ""Well hello there"",
                    ""complex"" : [{
            		    ""key"": """ + id3.ToString() + @""",
            		    ""name"": ""I am a sub nested content"",
            		    ""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
            		    ""title"": ""Hello up there :)"",
                        ""bodyText"": ""Hello way up there on a different level""
            	    }]
            	}
            ]";
            content.SetValue("complex", complexValue);

            // map the persisted properties to a model representing properties to save
            //var saveProperties = content.Properties.Select(x => Mapper.Map<ContentPropertyBasic>(x)).ToList();
            var saveProperties = content.Properties.Select(x =>
            {
                return new ContentPropertyBasic
                {
                    Alias = x.Alias,
                    Id = x.Id,
                    Value = x.GetValue()
                };
            }).ToList();

            var saveVariants = new List<ContentVariantSave>
            {
                new ContentVariantSave
                    {
                        Culture = string.Empty,
                        Segment = string.Empty,
                        Name = content.Name,
                        Save = true,
                        Properties = saveProperties
                    }
            };

            var save = new ContentItemSave
            {
                Id = content.Id,
                Action = ContentSaveAction.Save,
                ContentTypeAlias = _contentType.Alias,
                ParentId = -1,
                PersistedContent = content,
                TemplateAlias = null,
                Variants = saveVariants
            };

            // This will map the ContentItemSave.Variants.PropertyCollectionDto and then map the values in the saved model
            // back onto the persisted IContent model.
            ContentItemBinder.BindModel(save, content);

            var modelState = new ModelStateDictionary();
            var isValid = validator.ValidatePropertiesData(save, saveVariants[0], saveVariants[0].PropertyCollectionDto, modelState);

            // list results for debugging
            foreach (var state in modelState)
            {
                Console.WriteLine(state.Key);
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine("\t" + error.ErrorMessage);
                }
            }

            // assert

            Assert.IsFalse(isValid);
            Assert.AreEqual(11, modelState.Keys.Count);
            const string complexPropertyKey = "_Properties.complex.invariant.null";
            Assert.IsTrue(modelState.Keys.Contains(complexPropertyKey));
            foreach (var state in modelState.Where(x => x.Key != complexPropertyKey))
            {
                foreach (var error in state.Value.Errors)
                {
                    Assert.IsFalse(error.ErrorMessage.DetectIsJson()); // non complex is just an error message                    
                }
            }
            var complexEditorErrors = modelState.Single(x => x.Key == complexPropertyKey).Value.Errors;
            Assert.AreEqual(1, complexEditorErrors.Count);
            var nestedError = complexEditorErrors[0];
            var jsonError = JsonConvert.DeserializeObject<JArray>(nestedError.ErrorMessage);

            var modelStateKeys = new[] { "_Properties.title.invariant.null.innerFieldId", "_Properties.title.invariant.null.value", "_Properties.bodyText.invariant.null.innerFieldId", "_Properties.bodyText.invariant.null.value" };
            AssertNestedValidation(jsonError, 0, id1, modelStateKeys);
            AssertNestedValidation(jsonError, 1, id2, modelStateKeys.Concat(new[] { "_Properties.complex.invariant.null.innerFieldId", "_Properties.complex.invariant.null.value" }).ToArray());
            var nestedJsonError = jsonError.SelectToken("$[1].complex") as JArray;
            Assert.IsNotNull(nestedJsonError);
            AssertNestedValidation(nestedJsonError, 0, id3, modelStateKeys);
        }

        private void AssertNestedValidation(JArray jsonError, int index, Guid id, string[] modelStateKeys)
        {
            Assert.IsNotNull(jsonError.SelectToken("$[" + index + "]"));
            Assert.AreEqual(id.ToString(), jsonError.SelectToken("$[" + index + "].$id").Value<string>());
            Assert.AreEqual("textPage", jsonError.SelectToken("$[" + index + "].$elementTypeAlias").Value<string>());
            Assert.IsNotNull(jsonError.SelectToken("$[" + index + "].ModelState"));            
            foreach (var key in modelStateKeys)
            {
                var error = jsonError.SelectToken("$[" + index + "].ModelState['" + key + "']") as JArray;
                Assert.IsNotNull(error);
                Assert.AreEqual(1, error.Count);
            }
        }

        [HideFromTypeFinder]
        [DataEditor("complexTest", "test", "test")]
        public class ComplexTestEditor : NestedContentPropertyEditor
        {
            public ComplexTestEditor(ILogger logger, Lazy<PropertyEditorCollection> propertyEditors, IDataTypeService dataTypeService, IContentTypeService contentTypeService) : base(logger, propertyEditors, dataTypeService, contentTypeService)
            {
            }

            protected override IDataValueEditor CreateValueEditor()
            {
                var editor = base.CreateValueEditor();
                editor.Validators.Add(new NeverValidateValidator());
                return editor;
            }
        }

        [HideFromTypeFinder]
        [DataEditor("test", "test", "test")] // This alias aligns with the prop editor alias for all properties created from MockedContentTypes.CreateTextPageContentType
        public class TestEditor : DataEditor
        {
            public TestEditor(ILogger logger)
                : base(logger)
            {
            }

            protected override IDataValueEditor CreateValueEditor() => new TestValueEditor(Attribute);

            private class TestValueEditor : DataValueEditor
            {
                public TestValueEditor(DataEditorAttribute attribute)
                    : base(attribute)
                {
                    Validators.Add(new NeverValidateValidator());
                }

            }
        }

        public class NeverValidateValidator : IValueValidator
        {
            public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
            {
                yield return new ValidationResult("WRONG!", new[] { "innerFieldId" });
            }
        }

    }
}
