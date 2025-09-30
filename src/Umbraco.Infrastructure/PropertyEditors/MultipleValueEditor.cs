// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A value editor to handle posted JSON array data and to return array data for the multiple selected CSV items.
/// </summary>
/// <remarks>
///     This is re-used by editors such as the multiple drop down list or check box list.
/// </remarks>
public class MultipleValueEditor : DataValueEditor
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleValueEditor"/> class.
    /// </summary>
    public MultipleValueEditor(
        ILocalizedTextService localizedTextService,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
        : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
    {
        _jsonSerializer = jsonSerializer;
        Validators.Add(new MultipleValueValidator(localizedTextService));
    }

    /// <summary>
    ///     When multiple values are selected a JSON array will be posted back so we need to format for storage in
    ///     the database which is a comma separated string value.
    /// </summary>
    /// <returns></returns>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        if (editorValue.Value is not IEnumerable<string> stringValues || stringValues.Any() == false)
        {
            return null;
        }

        return _jsonSerializer.Serialize(stringValues);
    }
}
