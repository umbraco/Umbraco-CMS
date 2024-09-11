// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a checkbox property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Boolean,
    ValueType = ValueTypes.Integer,
    ValueEditorIsReusable = true)]
public class TrueFalsePropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TrueFalsePropertyEditor" /> class.
    /// </summary>
    public TrueFalsePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<TrueFalsePropertyValueEditor>(Attribute!);

    internal class TrueFalsePropertyValueEditor : DataValueEditor
    {
        public TrueFalsePropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
            => ParsePropertyValue(property.GetValue(culture, segment));

        // NOTE: property editor value type is Integer, which means we need to store the boolean representation as 0 or 1
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => ParsePropertyValue(editorValue.Value) ? 1 : 0;

        private bool ParsePropertyValue(object? value)
            => value switch
            {
                bool booleanValue => booleanValue,
                int integerValue => integerValue == 1,
                string stringValue => stringValue == "1" || stringValue.Equals("true", StringComparison.OrdinalIgnoreCase),
                _ => false
            };
    }
}
