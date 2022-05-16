﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a textarea property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.TextArea,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Textarea",
        "textarea",
        ValueType = ValueTypes.Text,
        Icon = "icon-application-window-alt")]
    public class TextAreaPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public TextAreaPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaPropertyEditor"/> class.
        /// </summary>
        public TextAreaPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<TextOnlyValueEditor>(Attribute!);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextAreaConfigurationEditor(_ioHelper, _editorConfigurationParser);
    }
}
