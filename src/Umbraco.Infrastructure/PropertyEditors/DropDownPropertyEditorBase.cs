// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a drop-down property editor used in Umbraco for selecting values from a configurable list.
/// </summary>
/// <remarks>
/// There is one drop-down editor per number of values it holds - a single value or several - so that the type a
/// drop-down property yields follows from the editor rather than from the configuration of the data type it is used
/// through. Both are edited and stored the same way, which is what this base holds.
/// </remarks>
public abstract class DropDownPropertyEditorBase : DataEditor, IValueSchemaProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownPropertyEditorBase"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property values.</param>
    protected DropDownPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(IEnumerable<string>);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("array", "null"),
        ["items"] = new JsonObject
        {
            ["type"] = "string",
        },
        ["description"] = "Array of selected values from dropdown",
    };

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleValueEditor>(Attribute!);
}
