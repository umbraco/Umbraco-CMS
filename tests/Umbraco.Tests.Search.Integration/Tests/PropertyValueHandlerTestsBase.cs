using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Integration.Tests;

public abstract class PropertyValueHandlerTestsBase : ContentTestBase
{
    protected async Task<IContentType> CreateAllSimpleEditorsContentType()
    {
        IDataTypeService dataTypeService = GetRequiredService<IDataTypeService>();

        DataType decimalDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Decimal)
            .WithName("Decimal")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(decimalDataType, Constants.Security.SuperUserKey);

        DataType tagsAsCsvDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Tags as CSV")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .Build();
        tagsAsCsvDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "storageType", TagsStorageType.Csv }
        };
        await dataTypeService.CreateAsync(tagsAsCsvDataType, Constants.Security.SuperUserKey);

        DataType tagsAsJsonDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Tags as JSON")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .Build();
        tagsAsJsonDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "storageType", TagsStorageType.Json }
        };
        await dataTypeService.CreateAsync(tagsAsCsvDataType, Constants.Security.SuperUserKey);

        DataType multipleTextstringsDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Multiple textstrings")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.MultipleTextstring)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(multipleTextstringsDataType, Constants.Security.SuperUserKey);

        DataType contentPickerDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Content picker")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(contentPickerDataType, Constants.Security.SuperUserKey);

        DataType sliderSingleDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Slider single")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Slider)
            .Done()
            .Build();
        sliderSingleDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "enableRange", false }
        };
        await dataTypeService.CreateAsync(sliderSingleDataType, Constants.Security.SuperUserKey);

        DataType sliderRangeDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Slider range")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Slider)
            .Done()
            .Build();
        sliderSingleDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "enableRange", true }
        };
        await dataTypeService.CreateAsync(sliderRangeDataType, Constants.Security.SuperUserKey);

        DataType multiUrlPickerDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Multi URL picker")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.MultiUrlPicker)
            .Done()
            .Build();
        await dataTypeService.CreateAsync(multiUrlPickerDataType, Constants.Security.SuperUserKey);

        DataType dropdownSingleDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Drop-down (single)")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .Done()
            .Build();
        dropdownSingleDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "multiple", false },
            { "items", new [] { "One", "Two", "Three" } }
        };
        await dataTypeService.CreateAsync(dropdownSingleDataType, Constants.Security.SuperUserKey);

        DataType dropdownMultipleDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Drop-down (multiple)")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .Done()
            .Build();
        dropdownMultipleDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "multiple", true },
            { "items", new [] { "One", "Two", "Three" } }
        };
        await dataTypeService.CreateAsync(dropdownMultipleDataType, Constants.Security.SuperUserKey);

        DataType radioButtonListDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("Radio button list")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.RadioButtonList)
            .Done()
            .Build();
        radioButtonListDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "items", new [] { "One", "Two", "Three" } }
        };
        await dataTypeService.CreateAsync(radioButtonListDataType, Constants.Security.SuperUserKey);

        DataType checkBoxListDataType = new DataTypeBuilder()
            .WithId(0)
            .WithDatabaseType(ValueStorageType.Nvarchar)
            .WithName("CheckBox list")
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.CheckBoxList)
            .Done()
            .Build();
        checkBoxListDataType.ConfigurationData = new Dictionary<string, object>
        {
            { "items", new [] { "One", "Two", "Three" } }
        };
        await dataTypeService.CreateAsync(checkBoxListDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("allSimpleEditors")
            .AddPropertyType()
            .WithAlias("textBoxValue")
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("textAreaValue")
            .WithDataTypeId(Constants.DataTypes.Textarea)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextArea)
            .Done()
            .AddPropertyType()
            .WithAlias("integerValue")
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalValue")
            .WithDataTypeId(decimalDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .AddPropertyType()
            .WithAlias("dateValue")
            .WithDataTypeId(-41)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("dateAndTimeValue")
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .AddPropertyType()
            .WithAlias("tagsAsJsonValue")
            .WithDataTypeId(tagsAsJsonDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .AddPropertyType()
            .WithAlias("tagsAsCsvValue")
            .WithDataTypeId(tagsAsCsvDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Tags)
            .Done()
            .AddPropertyType()
            .WithAlias("multipleTextstringsValue")
            .WithDataTypeId(multipleTextstringsDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MultipleTextstring)
            .Done()
            .AddPropertyType()
            .WithAlias("contentPickerValue")
            .WithDataTypeId(contentPickerDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.ContentPicker)
            .Done()
            .AddPropertyType()
            .WithAlias("booleanAsBooleanValue")
            .WithDataTypeId(Constants.DataTypes.Boolean)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("booleanAsIntegerValue")
            .WithDataTypeId(Constants.DataTypes.Boolean)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("booleanAsStringValue")
            .WithDataTypeId(Constants.DataTypes.Boolean)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("sliderSingleValue")
            .WithDataTypeId(sliderSingleDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Slider)
            .Done()
            .AddPropertyType()
            .WithAlias("sliderRangeValue")
            .WithDataTypeId(sliderRangeDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Slider)
            .Done()
            .AddPropertyType()
            .WithAlias("multiUrlPickerValue")
            .WithDataTypeId(multiUrlPickerDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.MultiUrlPicker)
            .Done()
            .AddPropertyType()
            .WithAlias("dropdownSingleValue")
            .WithDataTypeId(dropdownSingleDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .Done()
            .AddPropertyType()
            .WithAlias("dropdownMultipleValue")
            .WithDataTypeId(dropdownMultipleDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DropDownListFlexible)
            .Done()
            .AddPropertyType()
            .WithAlias("radioButtonListValue")
            .WithDataTypeId(radioButtonListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.RadioButtonList)
            .Done()
            .AddPropertyType()
            .WithAlias("checkBoxListValue")
            .WithDataTypeId(checkBoxListDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.CheckBoxList)
            .Done()
            .Build();

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return contentType;
    }

    protected IContentType GetAllSimpleEditorsContentType() => ContentTypeService.Get("allSimpleEditors")
                                                               ?? throw new InvalidOperationException("Could not find the content type \"allSimpleEditors\"");
}
