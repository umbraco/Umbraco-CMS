// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for read-only label properties.
/// </summary>
/// <remarks>
///     There is one label editor per type of value a label can hold, so that the type a label property yields is
///     fixed by the editor rather than by the configuration of the data type it is used through. The value type each
///     one stores is declared by its own <see cref="DataEditorAttribute" />.
/// </remarks>
public abstract class LabelPropertyEditorBase : DataEditor, IValueSchemaProvider
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LabelPropertyEditorBase" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    protected LabelPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        IOHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <summary>
    ///     Gets the IO helper.
    /// </summary>
    protected IIOHelper IOHelper { get; }

    /// <inheritdoc />
    /// <remarks>
    ///     A label ignores anything submitted to it, whatever it stores, so the incoming value is always a string.
    /// </remarks>
    public Type? GetValueType(object? configuration) => typeof(string);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["description"] = "Read-only value, any value provided will be ignored",
    };

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<LabelPropertyValueEditor>(Attribute!);

    /// <summary>
    /// Provides the property value editor for label properties.
    /// </summary>
    internal sealed class LabelPropertyValueEditor : DataValueEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelPropertyValueEditor"/> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        public LabelPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        /// <inheritdoc />
        public override bool IsReadOnly => true;
    }
}
