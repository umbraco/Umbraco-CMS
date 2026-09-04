using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for selecting members.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MemberPicker,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class MemberPickerPropertyEditor : DataEditor, IValueSchemaProvider
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public MemberPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : this(
            dataValueEditorFactory,
            StaticServiceProvider.Instance.GetRequiredService<IIOHelper>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public MemberPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    public Type? GetValueType(object? configuration) => typeof(Guid?);

    /// <inheritdoc />
    public JsonObject? GetValueSchema(object? configuration) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = new JsonArray("string", "null"),
        ["format"] = "uuid",
        ["pattern"] = ValueSchemaPatterns.Uuid,
        ["description"] = "GUID of the selected member",
    };

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MemberPickerConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MemberPickerPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Provides the value editor for the member picker property editor.
    /// </summary>
    internal sealed class MemberPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        private readonly IMemberService _memberService;

        public MemberPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute,
            IMemberService memberService,
            ILocalizedTextService localizedTextService,
            ICoreScopeProvider coreScopeProvider)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
            _memberService = memberService;
            Validators.Add(new TypedValidatorRunner<string, MemberPickerConfigurationBase>(
                new SingleMemberTypeFilterValidator(localizedTextService, memberService, coreScopeProvider)));
        }

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // the stored value is either an UDI or an integer ID - need to transform this into the corresponding member key
            var value = base.ToEditor(property, culture, segment);
            if (value is not string stringValue || stringValue.IsNullOrWhiteSpace())
            {
                return value;
            }

            if (UdiParser.TryParse<GuidUdi>(stringValue, out GuidUdi? guidUdi))
            {
                return guidUdi.Guid;
            }

            if (int.TryParse(stringValue, out int memberId))
            {
                return _memberService.GetById(memberId)?.Key;
            }

            return null;
        }

        // the editor value is expected to be the member key - store it as the member UDI
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            => editorValue.Value is string stringValue && Guid.TryParse(stringValue, out Guid memberKey)
                ? new GuidUdi(Constants.UdiEntityType.Member, memberKey)
                : null;

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value is string str ? str : value?.ToString();

            if (string.IsNullOrEmpty(asString))
            {
                yield break;
            }

            if (UdiParser.TryParse(asString, out Udi? udi))
            {
                yield return new UmbracoEntityReference(udi);
            }
        }
    }
}
