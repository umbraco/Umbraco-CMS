// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for label properties holding a time.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.LabelTime,
    ValueType = ValueTypes.Time,
    ValueEditorIsReusable = true)]
public class TimeLabelPropertyEditor : LabelPropertyEditorBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TimeLabelPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public TimeLabelPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory, ioHelper)
    {
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<TimeLabelPropertyValueEditor>(Attribute!);

    /// <summary>
    /// Provides the property value editor for label properties holding a time.
    /// </summary>
    internal sealed class TimeLabelPropertyValueEditor : LabelPropertyValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeLabelPropertyValueEditor"/> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        public TimeLabelPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        /// <inheritdoc />
        /// <remarks>
        ///     A time is stored in a date column, so only its time of day carries any meaning.
        /// </remarks>
        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);

            Attempt<DateTime?> time = value.TryConvertTo<DateTime?>();

            return time is { Success: true, Result: not null }
                ? time.Result.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                : string.Empty;
        }
    }
}
