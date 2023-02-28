// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
    "Multinode Treepicker",
    "contentpicker",
    ValueType = ValueTypes.Text,
    Group = Constants.PropertyEditors.Groups.Pickers,
    Icon = "icon-page-add",
    ValueEditorIsReusable = true)]
public class MultiNodeTreePickerPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MultiNodeTreePickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public MultiNodeTreePickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiNodePickerConfigurationEditor(_ioHelper, _editorConfigurationParser);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiNodeTreePickerPropertyValueEditor>(Attribute!);

    public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public MultiNodeTreePickerPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            var udiPaths = asString!.Split(',');
            foreach (var udiPath in udiPaths)
            {
                if (UdiParser.TryParse(udiPath, out Udi? udi))
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }
        }

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);
            return value is string stringValue
                ? ParseValidUdis(stringValue.Split(Constants.CharArrays.Comma))
                : null;
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is IEnumerable<string> stringValues
                ? string.Join(",", ParseValidUdis(stringValues))
                : null;

        private string[] ParseValidUdis(IEnumerable<string> stringValues)
            => stringValues
                .Select(s => UdiParser.TryParse(s, out Udi? udi) && udi is GuidUdi guidUdi ? guidUdi.ToString() : null)
                .WhereNotNull()
                .ToArray();
    }
}
