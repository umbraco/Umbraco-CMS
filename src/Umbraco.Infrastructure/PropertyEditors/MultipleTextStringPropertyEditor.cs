// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a multiple text string property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultipleTextstring,
    "Repeatable textstrings",
    "multipletextbox",
    ValueType = ValueTypes.Text,
    Group = Constants.PropertyEditors.Groups.Lists,
    Icon = "icon-ordered-list",
    ValueEditorIsReusable = true)]
public class MultipleTextStringPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MultipleTextStringPropertyEditor(
        IIOHelper ioHelper,
        IDataValueEditorFactory dataValueEditorFactory)
        : this(ioHelper, dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleTextStringPropertyEditor" /> class.
    /// </summary>
    public MultipleTextStringPropertyEditor(
        IIOHelper ioHelper,
        IDataValueEditorFactory dataValueEditorFactory,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultipleTextStringPropertyValueEditor>(Attribute!);

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultipleTextStringConfigurationEditor(_ioHelper, _editorConfigurationParser);

    /// <summary>
    ///     Custom value editor so we can format the value for the editor and the database
    /// </summary>
    internal class MultipleTextStringPropertyValueEditor : DataValueEditor
    {
        private readonly ILocalizedTextService _localizedTextService;

        public MultipleTextStringPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) =>
            _localizedTextService = localizedTextService;

        /// <summary>
        ///     A custom FormatValidator is used as for multiple text strings, each string should individually be checked
        ///     against the configured regular expression, rather than the JSON representing all the strings as a whole.
        /// </summary>
        public override IValueFormatValidator FormatValidator =>
            new MultipleTextStringFormatValidator(_localizedTextService);

        /// <summary>
        ///     The value passed in from the editor will be an array of simple objects so we'll need to parse them to get the
        ///     string
        /// </summary>
        /// <param name="editorValue"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        /// <remarks>
        ///     We will also check the pre-values here, if there are more items than what is allowed we'll just trim the end
        /// </remarks>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is not JArray asArray || asArray.HasValues == false)
            {
                return null;
            }

            if (!(editorValue.DataTypeConfiguration is MultipleTextStringConfiguration config))
            {
                throw new PanicException(
                    $"editorValue.DataTypeConfiguration is {editorValue.DataTypeConfiguration?.GetType()} but must be {typeof(MultipleTextStringConfiguration)}");
            }

            var max = config.Maximum;

            // The legacy property editor saved this data as new line delimited! strange but we have to maintain that.
            IEnumerable<string?> array = asArray.OfType<JObject>()
                .Where(x => x["value"] != null)
                .Select(x => x["value"]!.Value<string>());

            // only allow the max if over 0
            if (max > 0)
            {
                return string.Join(Environment.NewLine, array.Take(max));
            }

            return string.Join(Environment.NewLine, array);
        }

        /// <summary>
        ///     We are actually passing back an array of simple objects instead of an array of strings because in angular a
        ///     primitive (string) value
        ///     cannot have 2 way binding, so to get around that each item in the array needs to be an object with a string.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="culture"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        /// <remarks>
        ///     The legacy property editor saved this data as new line delimited! strange but we have to maintain that.
        /// </remarks>
        public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            var val = property.GetValue(culture, segment);
            return val?.ToString()?.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => JObject.FromObject(new { value = x })) ?? new JObject[] { };
        }
    }

    internal class MultipleTextStringFormatValidator : IValueFormatValidator
    {
        private readonly ILocalizedTextService _localizedTextService;

        public MultipleTextStringFormatValidator(ILocalizedTextService localizedTextService) =>
            _localizedTextService = localizedTextService;

        public IEnumerable<ValidationResult> ValidateFormat(object? value, string valueType, string format)
        {
            if (value is not JArray asArray)
            {
                return Enumerable.Empty<ValidationResult>();
            }

            IEnumerable<string?> textStrings = asArray.OfType<JObject>()
                .Where(x => x["value"] != null)
                .Select(x => x["value"]!.Value<string>());
            var textStringValidator = new RegexValidator(_localizedTextService);
            foreach (var textString in textStrings)
            {
                var validationResults = textStringValidator.ValidateFormat(textString, valueType, format).ToList();
                if (validationResults.Any())
                {
                    return validationResults;
                }
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
