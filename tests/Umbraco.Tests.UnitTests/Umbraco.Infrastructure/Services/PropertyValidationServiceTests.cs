// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Services;

[TestFixture]
public class PropertyValidationServiceTests
{
    private IShortStringHelper ShortStringHelper => new DefaultShortStringHelper(new DefaultShortStringHelperConfig());

    private void MockObjects(out PropertyValidationService validationService, out IDataType dt)
    {
        var textService = new Mock<ILocalizedTextService>();
        textService.Setup(x =>
                x.Localize(It.IsAny<string>(), It.IsAny<string>(), Thread.CurrentThread.CurrentCulture, null))
            .Returns("Localized text");

        var dataTypeService = new Mock<IDataTypeService>();
        var dataType = Mock.Of<IDataType>(
            x => x.Configuration == string.Empty // irrelevant but needs a value
                 && x.DatabaseType == ValueStorageType.Nvarchar
                 && x.EditorAlias == Constants.PropertyEditors.Aliases.TextBox);
        dataTypeService.Setup(x => x.GetDataType(It.IsAny<int>())).Returns(() => dataType);
        dt = dataType;

        // new data editor that returns a TextOnlyValueEditor which will do the validation for the properties
        var dataEditor = Mock.Of<IDataEditor>(
            x => x.Type == EditorType.PropertyValue
                 && x.Alias == Constants.PropertyEditors.Aliases.TextBox);
        Mock.Get(dataEditor).Setup(x => x.GetValueEditor(It.IsAny<object>()))
            .Returns(new CustomTextOnlyValueEditor(
                new DataEditorAttribute(Constants.PropertyEditors.Aliases.TextBox, "Test Textbox", "textbox"),
                textService.Object,
                Mock.Of<IShortStringHelper>(),
                new JsonNetSerializer(),
                Mock.Of<IIOHelper>()));

        var propEditors = new PropertyEditorCollection(new DataEditorCollection(() => new[] { dataEditor }));

        validationService = new PropertyValidationService(propEditors, dataTypeService.Object, Mock.Of<ILocalizedTextService>(), new ValueEditorCache());
    }

    [Test]
    public void Validate_Invariant_Properties_On_Variant_Default_Culture()
    {
        MockObjects(out var validationService, out var dataType);

        var p1 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test1")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p1.SetValue("Hello", "en-US");
        var p2 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test2")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p2.SetValue("Hello");
        var p3 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test3")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p3.SetValue(null, "en-US"); // invalid
        var p4 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test4")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p4.SetValue(null); // invalid

        var content = Mock.Of<IContent>(
            x => x.Published == true // set to published, the default culture will validate invariant anyways
                 && x.Properties == new PropertyCollection(new[] { p1, p2, p3, p4 }));

        var result =
            validationService.IsPropertyDataValid(content, out var invalid, CultureImpact.Explicit("en-US", true));

        Assert.IsFalse(result);
        Assert.AreEqual(2, invalid.Length);
    }

    [Test]
    public void Validate_Invariant_Properties_On_Variant_Non_Default_Culture()
    {
        MockObjects(out var validationService, out var dataType);

        var p1 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test1")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p1.SetValue("Hello", "en-US");
        var p2 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test2")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p2.SetValue("Hello");
        var p3 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test3")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p3.SetValue(null, "en-US"); // invalid
        var p4 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test4")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p4.SetValue(null); // invalid

        var content = Mock.Of<IContent>(
            x =>
                x.Published ==
                false // set to not published, the non default culture will need to validate invariant too
                && x.Properties == new PropertyCollection(new[] { p1, p2, p3, p4 }));

        var result =
            validationService.IsPropertyDataValid(content, out var invalid, CultureImpact.Explicit("en-US", false));

        Assert.IsFalse(result);
        Assert.AreEqual(2, invalid.Length);
    }

    [Test]
    public void Validate_Variant_Properties_On_Variant()
    {
        MockObjects(out var validationService, out var dataType);

        var p1 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test1")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p1.SetValue(null, "en-US"); // invalid
        var p2 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test2")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p2.SetValue(null); // invalid
        var p3 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test3")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p3.SetValue(null, "en-US"); // ignored because the impact isn't the default lang + the content is published
        var p4 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test4")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p4.SetValue(null); // ignored because the impact isn't the default lang + the content is published

        var content = Mock.Of<IContent>(
            x => x.Published == true // set to published
                 && x.Properties == new PropertyCollection(new[] { p1, p2, p3, p4 }));

        var result =
            validationService.IsPropertyDataValid(content, out var invalid, CultureImpact.Explicit("en-US", false));

        Assert.IsFalse(result);
        Assert.AreEqual(2, invalid.Length);
    }

    [Test]
    public void Validate_Invariant_Properties_On_Invariant()
    {
        MockObjects(out var validationService, out var dataType);

        var p1 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test1")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p1.SetValue(null, "en-US"); // ignored since this is variant
        var p2 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test2")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p2.SetValue(null); // invalid
        var p3 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test3")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p3.SetValue("Hello", "en-US"); // ignored since this is variant
        var p4 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test4")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p4.SetValue(null); // invalid

        var content = Mock.Of<IContent>(
            x => x.Properties == new PropertyCollection(new[] { p1, p2, p3, p4 }));

        var result = validationService.IsPropertyDataValid(content, out var invalid, CultureImpact.Invariant);

        Assert.IsFalse(result);
        Assert.AreEqual(2, invalid.Length);
    }

    [Test]
    public void Validate_Properties_On_All()
    {
        MockObjects(out var validationService, out var dataType);

        var p1 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test1")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p1.SetValue(null, "en-US"); // invalid
        var p2 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test2")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p2.SetValue(null); // invalid
        var p3 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test3")
            {
                Mandatory = true,
                Variations = ContentVariation.Culture,
            });
        p3.SetValue(null, "en-US"); // invalid
        var p4 = new Property(
            new PropertyType(ShortStringHelper, dataType, "test4")
            {
                Mandatory = true,
                Variations = ContentVariation.Nothing,
            });
        p4.SetValue(null); // invalid

        var content = Mock.Of<IContent>(
            x => x.Properties == new PropertyCollection(new[] { p1, p2, p3, p4 }));

        var result = validationService.IsPropertyDataValid(content, out var invalid, CultureImpact.All);

        Assert.IsFalse(result);
        Assert.AreEqual(4, invalid.Length);
    }

    // used so we can inject a mock - we should fix the base class DataValueEditor to be able to have the ILocalizedTextField passed
    // in to create the Requried and Regex validators so we aren't using singletons
    private class CustomTextOnlyValueEditor : TextOnlyValueEditor
    {
        private readonly ILocalizedTextService _textService;

        public CustomTextOnlyValueEditor(
            DataEditorAttribute attribute,
            ILocalizedTextService textService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper)
            : base(attribute, textService, shortStringHelper, jsonSerializer, ioHelper) => _textService = textService;

        public override IValueRequiredValidator RequiredValidator => new RequiredValidator(_textService);

        public override IValueFormatValidator FormatValidator => new RegexValidator(_textService, null);
    }
}
