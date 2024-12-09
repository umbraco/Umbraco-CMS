// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a slider editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Slider,
    ValueEditorIsReusable = true)]
public class SliderPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SliderPropertyEditor" /> class.
    /// </summary>
    public SliderPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<SliderPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SliderConfigurationEditor(_ioHelper);

    internal class SliderPropertyValueEditor : DataValueEditor
    {
        private readonly IJsonSerializer _jsonSerializer;

        public SliderPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute) =>
            _jsonSerializer = jsonSerializer;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // value is stored as a string - either a single decimal value
            // or a two decimal values separated by comma (for range sliders)
            var value = property.GetValue(culture, segment);
            if (value is not string stringValue)
            {
                return null;
            }

            var parts = stringValue.Split(Constants.CharArrays.Comma);
            var parsed = parts
                .Select(s => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var i) ? i : (decimal?)null)
                .Where(i => i != null)
                .Select(i => i!.Value)
                .ToArray();

            return parts.Length == parsed.Length && parsed.Length is 1 or 2
                ? new SliderRange { From = parsed.First(), To = parsed.Last() }
                : null;
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is not null && _jsonSerializer.TryDeserialize(editorValue.Value, out SliderRange? sliderRange)
                ? sliderRange.ToString()
                : null;

        internal class SliderRange
        {
            public decimal From { get; set; }

            public decimal To { get; set; }

            public override string ToString() => From == To ? $"{From.ToString(CultureInfo.InvariantCulture)}" : $"{From.ToString(CultureInfo.InvariantCulture)},{To.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
