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
        public void TestSerializer()
        {
            var nestedLevel2 = new NestedValidationResults();
            var elementTypeResult2 = new ElementTypeValidationResult("type2");
            var propertyTypeResult2 = new PropertyTypeValidationResult("prop2");
            propertyTypeResult2.ValidationResults.Add(new ValidationResult("error2-1", new[] { "level2" }));
            propertyTypeResult2.ValidationResults.Add(new ValidationResult("error2-2", new[] { "level2" }));
            elementTypeResult2.ValidationResults.Add(propertyTypeResult2);
            nestedLevel2.ValidationResults.Add(elementTypeResult2);

            var nestedLevel1 = new NestedValidationResults();
            var elementTypeResult1 = new ElementTypeValidationResult("type1");
            var propertyTypeResult1 = new PropertyTypeValidationResult("prop1");
            propertyTypeResult1.ValidationResults.Add(new ValidationResult("error1-1", new[] { "level1" }));
            propertyTypeResult1.ValidationResults.Add(nestedLevel2); // This is a nested result within the level 1
            elementTypeResult1.ValidationResults.Add(propertyTypeResult1);
            nestedLevel1.ValidationResults.Add(elementTypeResult1);

            var serialized = JsonConvert.SerializeObject(nestedLevel1, Formatting.Indented, new ValidationResultConverter());
            Console.WriteLine(serialized);
        }

        [Test]
        public void Test()
        {
            var validator = new ContentSaveModelValidator(
                Factory.GetInstance<ILogger>(),
                Mock.Of<IUmbracoContextAccessor>(),
                Factory.GetInstance<ILocalizedTextService>());

            var content = MockedContent.CreateTextpageContent(_contentType, "test", -1);

            // TODO: Ok now test with a 4th level complex nested editor

            const string complexValue = @"[{
            		""key"": ""c8df5136-d606-41f0-9134-dea6ae0c2fd9"",
            		""name"": ""Hello world"",
            		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
            		""title"": ""Hello world"",
                    ""bodyText"": ""The world is round""
            	}, {
            		""key"": ""f916104a-4082-48b2-a515-5c4bf2230f38"",
            		""name"": ""Super nested"",
            		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
            		""title"": ""Hi there!"",
                    ""bodyText"": ""Well hello there"",
                    ""complex"" : [{
            		    ""key"": ""77E15DE9-1C79-47B2-BC60-4913BC4D4C6A"",
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
            Assert.AreEqual(5, modelState.Keys.Count);
            const string complexPropertyKey = "_Properties.complex.invariant.null";
            Assert.IsTrue(modelState.Keys.Contains(complexPropertyKey));
            foreach (var state in modelState.Where(x => x.Key != complexPropertyKey))
            {
                foreach (var error in state.Value.Errors)
                {
                    Assert.IsTrue(error.ErrorMessage.DetectIsJson());
                    var json = JsonConvert.DeserializeObject<JObject>(error.ErrorMessage);
                    Assert.IsNotEmpty(json["errorMessage"].Value<string>());
                    Assert.AreEqual(1, json["memberNames"].Value<JArray>().Count);
                }
            }
            var complexEditorErrors = modelState.Single(x => x.Key == complexPropertyKey).Value.Errors;
            Assert.AreEqual(3, complexEditorErrors.Count);
            var nestedError = complexEditorErrors.Single(x => x.ErrorMessage.Contains("nestedValidation"));
            var jsonNestedError = JsonConvert.DeserializeObject<JObject>(nestedError.ErrorMessage);
            Assert.AreEqual(JTokenType.Array, jsonNestedError["nestedValidation"].Type);
            var nestedValidation = (JArray)jsonNestedError["nestedValidation"];
            AssertNestedValidation(nestedValidation, 2); // there are 2 because there are 2 nested content rows
        }

        private void AssertNestedValidation(JArray nestedValidation, int rows)
        {
            Assert.AreEqual(rows, nestedValidation.Count); 
            foreach (var rowErrors in nestedValidation)
            {
                Assert.AreEqual(JTokenType.Object, rowErrors.Type);
                var elementTypeErrors = (JObject)rowErrors; // this is a dictionary of element type alias -> dictionary of errors -> prop alias -> array errors
                Assert.AreEqual(1, elementTypeErrors.Count); // there is 1 element type in error
                foreach (var elementTypeAliasToErrors in elementTypeErrors)
                {
                    Assert.AreEqual("textPage", elementTypeAliasToErrors.Key);
                    var propErrors = (JObject)elementTypeAliasToErrors.Value;

                    foreach (var propAliasToErrors in propErrors)
                    {
                        Assert.AreEqual(JTokenType.Array, propAliasToErrors.Value.Type);
                        var propTypeErrors = (JArray)propAliasToErrors.Value;
                        
                        foreach (var propError in propTypeErrors)
                        {
                            var nested = propError["nestedValidation"];
                            if (nested != null)
                            {
                                // recurse
                                AssertNestedValidation((JArray)nested, 1); // we know this is 1 row
                                continue;
                            }

                            Assert.IsNotEmpty(propError["errorMessage"].Value<string>());
                            Assert.AreEqual(1, propError["memberNames"].Value<JArray>().Count);
                        }

                    }

                }


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
