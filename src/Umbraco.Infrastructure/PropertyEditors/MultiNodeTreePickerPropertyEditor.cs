// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Nodes;
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

    /// <remarks>
    /// At first glance, the fromEditor and toEditor methods might seem strange.
    /// This is because we wanted to stop the leaking of UDI's to the frontend while not having to do database migrations
    /// so we opted to, for now, translate the udi string in the database into a structured format unique to the client
    /// This way, for now, no migration is needed and no changes outside of the editor logic needs to be touched to stop the leaking.
    /// </remarks>
    public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IJsonSerializer _jsonSerializer;

        public MultiNodeTreePickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _jsonSerializer = jsonSerializer;
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


        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is JsonArray jsonArray
                ? EntityReferencesToUdis(_jsonSerializer.Deserialize<IEnumerable<EditorEntityReference>>(jsonArray.ToJsonString()) ?? Enumerable.Empty<EditorEntityReference>())
                : null;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var value = property.GetValue(culture, segment);
            return value is string stringValue
            ? UdisToEntityReferences(stringValue.Split(Constants.CharArrays.Comma)).ToArray()
            : null;
        }

        private IEnumerable<EditorEntityReference> UdisToEntityReferences(IEnumerable<string> stringUdis)
        {
            foreach (var stringUdi in stringUdis)
            {
                if (UdiParser.TryParse(stringUdi, out GuidUdi? guidUdi) is false)
                {
                    continue;
                }

                yield return new EditorEntityReference() { Type = guidUdi.EntityType, Unique = guidUdi.Guid };
            }
        }

        private string EntityReferencesToUdis(IEnumerable<EditorEntityReference> nodeReferences)
            => string.Join(",", nodeReferences.Select(entityReference => Udi.Create(entityReference.Type, entityReference.Unique).ToString()));

        public class EditorEntityReference
        {
            public required string Type { get; set; }

            public required Guid Unique { get; set; }
        }
    }
}
