﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a date and time property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.DateTime,
        "Date/Time",
        "datepicker",
        ValueType = ValueTypes.DateTime,
        Icon = "icon-time")]
    public class DateTimePropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePropertyEditor"/> class.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DateTimeValidator());
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new DateTimeConfigurationEditor(_ioHelper);
    }
}
