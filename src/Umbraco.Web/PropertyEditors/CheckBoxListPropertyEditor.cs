﻿using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow multiple checkbox selection of pre-defined items.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.CheckBoxList, "Checkbox list", "checkboxlist", Icon="icon-bulleted-list", Group="lists")]
    public class CheckBoxListPropertyEditor : DataEditor
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public CheckBoxListPropertyEditor(ILogger logger, ILocalizedTextService textService)
            : base(logger)
        {
            _textService = textService;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_textService);

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new MultipleValueEditor(Logger, Attribute);
    }
}
