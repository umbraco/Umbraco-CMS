// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
    ValueType = ValueTypes.Text,
    ValueEditorIsReusable = true)]
public class MultiNodeTreePickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    public MultiNodeTreePickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiNodePickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiNodeTreePickerPropertyValueEditor>(Attribute!);

    public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public MultiNodeTreePickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
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
