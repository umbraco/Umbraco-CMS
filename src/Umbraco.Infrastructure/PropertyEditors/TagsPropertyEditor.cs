﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a tags property editor.
    /// </summary>
    [TagsPropertyEditor]
    [DataEditor(
        Constants.PropertyEditors.Aliases.Tags,
        "Tags",
        "tags",
        Icon = "icon-tags")]
    public class TagsPropertyEditor : DataEditor
    {
        private readonly ManifestValueValidatorCollection _validators;
        private readonly IIOHelper _ioHelper;
        private readonly ILocalizedTextService _localizedTextService;

        public TagsPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            ManifestValueValidatorCollection validators,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService)
            : base(dataValueEditorFactory)
        {
            _validators = validators;
            _ioHelper = ioHelper;
            _localizedTextService = localizedTextService;
        }

        protected override IDataValueEditor CreateValueEditor() =>
            DataValueEditorFactory.Create<TagPropertyValueEditor>(Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new TagConfigurationEditor(_validators, _ioHelper, _localizedTextService);

        internal class TagPropertyValueEditor : DataValueEditor
        {
            public TagPropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            { }

            /// <inheritdoc />
            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                var value = editorValue?.Value?.ToString();

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                if (editorValue.Value is JArray json)
                {
                    return json.Select(x => x.Value<string>());
                }

                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    return value.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
                }

                return null;
            }

            /// <inheritdoc />
            public override IValueRequiredValidator RequiredValidator => new RequiredJsonValueValidator();

            /// <summary>
            /// Custom validator to validate a required value against an empty json value.
            /// </summary>
            /// <remarks>
            /// <para>This validator is required because the default RequiredValidator uses ValueType to
            /// determine whether a property value is JSON, and for tags the ValueType is string although
            /// the underlying data is JSON. Yes, this makes little sense.</para>
            /// </remarks>
            private class RequiredJsonValueValidator : IValueRequiredValidator
            {
                /// <inheritdoc />
                public IEnumerable<ValidationResult> ValidateRequired(object value, string valueType)
                {
                    if (value == null)
                    {
                        yield return new ValidationResult("Value cannot be null", new[] {"value"});
                        yield break;
                    }

                    if (value.ToString().DetectIsEmptyJson())
                        yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                }
            }
        }
    }
}
