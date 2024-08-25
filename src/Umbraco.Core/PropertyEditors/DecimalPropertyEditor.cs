using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a decimal property and parameter editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Decimal,
    ValueType = ValueTypes.Decimal,
    ValueEditorIsReusable = true)]
public class DecimalPropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalPropertyEditor" /> class.
    /// </summary>
    public DecimalPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory) =>
        SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<DecimalPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() => new DecimalConfigurationEditor();

    internal class DecimalPropertyValueEditor : DataValueEditor
    {
        public DecimalPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute) =>
            Validators.Add(new DecimalValidator());

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
            => TryParsePropertyValue(property.GetValue(culture, segment));

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => TryParsePropertyValue(editorValue.Value);

        private decimal? TryParsePropertyValue(object? value)
            => value is decimal decimalValue
                ? decimalValue
                : decimal.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out var parsedDecimalValue)
                    ? parsedDecimalValue
                    : null;
    }
}
