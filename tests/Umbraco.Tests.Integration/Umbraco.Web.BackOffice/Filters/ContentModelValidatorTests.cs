// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.ModelBinders;
using Umbraco.Extensions;
using DataType = Umbraco.Cms.Core.Models.DataType;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Filters;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Mapper = true, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
public class ContentModelValidatorTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        var complexEditorConfig = new NestedContentConfiguration
        {
            ContentTypes = new[] { new NestedContentConfiguration.ContentType { Alias = "feature" } }
        };

        var complexTestEditor = Services.GetRequiredService<ComplexTestEditor>();
        var testEditor = Services.GetRequiredService<TestEditor>();
        var dataTypeService = Services.GetRequiredService<IDataTypeService>();
        var serializer = Services.GetRequiredService<IConfigurationEditorJsonSerializer>();

        var complexDataType = new DataType(complexTestEditor, serializer)
        {
            Name = "ComplexTest",
            Configuration = complexEditorConfig
        };

        var testDataType = new DataType(testEditor, serializer) { Name = "Test" };
        dataTypeService.Save(complexDataType);
        dataTypeService.Save(testDataType);

        var fileService = Services.GetRequiredService<IFileService>();
        var template = TemplateBuilder.CreateTextPageTemplate();
        fileService.SaveTemplate(template);

        _contentType = ContentTypeBuilder.CreateTextPageContentType(ContentTypeAlias, defaultTemplateId: template.Id);

        // add complex editor
        foreach (var pt in _contentType.PropertyTypes)
        {
            pt.DataTypeId = testDataType.Id;
        }

        _contentType.AddPropertyType(
            new PropertyType(_shortStringHelper, "complexTest", ValueStorageType.Ntext)
            {
                Alias = "complex",
                Name = "Complex",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = complexDataType.Id
            },
            "content",
            "Content");

        // make them all validate with a regex rule that will not pass
        foreach (var prop in _contentType.PropertyTypes)
        {
            prop.ValidationRegExp = "^donotmatch$";
            prop.ValidationRegExpMessage = "Does not match!";
        }

        var contentTypeService = Services.GetRequiredService<IContentTypeService>();
        contentTypeService.Save(_contentType);
    }

    private const string ContentTypeAlias = "textPage";
    private IContentType _contentType;
    private readonly ContentModelBinderHelper _modelBinderHelper = new();

    private readonly IShortStringHelper _shortStringHelper =
        new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    //// protected override void Compose()
    //// {
    ////     base.Compose();
    ////
    ////     var complexEditorConfig = new NestedContentConfiguration
    ////     {
    ////         ContentTypes = new[]
    ////         {
    ////             new NestedContentConfiguration.ContentType { Alias = "feature" }
    ////         }
    ////     };
    ////     var dataTypeService = new Mock<IDataTypeService>();
    ////     dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>()))
    ////         .Returns((int id) => id == ComplexDataTypeId
    ////                 ? Mock.Of<IDataType>(x => x.Configuration == complexEditorConfig)
    ////                 : Mock.Of<IDataType>());
    ////
    ////     var contentTypeService = new Mock<IContentTypeService>();
    ////     contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>()))
    ////         .Returns(() => new List<IContentType>
    ////         {
    ////             _contentType
    ////         });
    ////
    ////     var textService = new Mock<ILocalizedTextService>();
    ////     textService.Setup(x => x.Localize("validation/invalidPattern", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns(() => "invalidPattern");
    ////     textService.Setup(x => x.Localize("validation/invalidNull", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns("invalidNull");
    ////     textService.Setup(x => x.Localize("validation/invalidEmpty", It.IsAny<CultureInfo>(), It.IsAny<IDictionary<string, string>>())).Returns("invalidEmpty");
    ////
    ////     composition.Services.AddUnique(x => Mock.Of<IDataTypeService>(x => x.GetDataType(It.IsAny<int>()) == Mock.Of<IDataType>()));
    ////     composition.Services.AddUnique(x => dataTypeService.Object);
    ////     composition.Services.AddUnique(x => contentTypeService.Object);
    ////     composition.Services.AddUnique(x => textService.Object);
    ////
    ////     Composition.WithCollectionBuilder<DataEditorCollectionBuilder>()
    ////         .Add<TestEditor>()
    ////         .Add<ComplexTestEditor>();
    //// }

    [Test]
    public void Validating_ContentItemSave()
    {
        var logger = Services.GetRequiredService<ILogger<ContentSaveModelValidator>>();
        var propertyValidationService = Services.GetRequiredService<IPropertyValidationService>();
        var umbracoMapper = Services.GetRequiredService<IUmbracoMapper>();

        var validator = new ContentSaveModelValidator(logger, propertyValidationService);

        var content = ContentBuilder.CreateTextpageContent(_contentType, "test", -1);

        var id1 = new Guid("c8df5136-d606-41f0-9134-dea6ae0c2fd9");
        var id2 = new Guid("f916104a-4082-48b2-a515-5c4bf2230f38");
        var id3 = new Guid("77E15DE9-1C79-47B2-BC60-4913BC4D4C6A");

        // TODO: Ok now test with a 4th level complex nested editor

        var complexValue = @"[{
             		""key"": """ + id1 + @""",
             		""name"": ""Hello world"",
             		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
             		""title"": ""Hello world"",
                     ""bodyText"": ""The world is round""
             	}, {
             		""key"": """ + id2 + @""",
             		""name"": ""Super nested"",
             		""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
             		""title"": ""Hi there!"",
                     ""bodyText"": ""Well hello there"",
                     ""complex"" : [{
             		    ""key"": """ + id3 + @""",
             		    ""name"": ""I am a sub nested content"",
             		    ""ncContentTypeAlias"": """ + ContentTypeAlias + @""",
             		    ""title"": ""Hello up there :)"",
                         ""bodyText"": ""Hello way up there on a different level""
             	    }]
             	}
             ]";
        content.SetValue("complex", complexValue);

        // map the persisted properties to a model representing properties to save
        // var saveProperties = content.Properties.Select(x => Mapper.Map<ContentPropertyBasic>(x)).ToList();
        var saveProperties = content.Properties.Select(x =>
        {
            return new ContentPropertyBasic { Alias = x.Alias, Id = x.Id, Value = x.GetValue() };
        }).ToList();

        var saveVariants = new List<ContentVariantSave>
        {
            new()
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
        ContentItemBinder.BindModel(save, content, _modelBinderHelper, umbracoMapper);

        var modelState = new ModelStateDictionary();
        var isValid =
            validator.ValidatePropertiesData(save, saveVariants[0], saveVariants[0].PropertyCollectionDto, modelState);

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
        Assert.AreEqual(11, modelState.Keys.Count());
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

        string[] modelStateKeys =
        {
            "_Properties.title.invariant.null.innerFieldId", "_Properties.title.invariant.null.value",
            "_Properties.bodyText.invariant.null.innerFieldId", "_Properties.bodyText.invariant.null.value"
        };
        AssertNestedValidation(jsonError, 0, id1, modelStateKeys);
        AssertNestedValidation(
            jsonError,
            1,
            id2,
            modelStateKeys.Concat(new[]
            {
                "_Properties.complex.invariant.null.innerFieldId", "_Properties.complex.invariant.null.value"
            }).ToArray());
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

    // [HideFromTypeFinder]
    [DataEditor("complexTest", "test", "test")]
    public class ComplexTestEditor : NestedContentPropertyEditor
    {
        public ComplexTestEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory, ioHelper, editorConfigurationParser)
        {
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new NeverValidateValidator());
            return editor;
        }
    }

    // [HideFromTypeFinder]
    [DataEditor("test", "test", "test")] // This alias aligns with the prop editor alias for all properties created from MockedContentTypes.CreateTextPageContentType
    public class TestEditor : DataEditor
    {
        public TestEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
        }

        protected override IDataValueEditor CreateValueEditor() =>
            DataValueEditorFactory.Create<TestValueEditor>(Attribute);

        private class TestValueEditor : DataValueEditor
        {
            public TestValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) =>
                Validators.Add(new NeverValidateValidator());
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
