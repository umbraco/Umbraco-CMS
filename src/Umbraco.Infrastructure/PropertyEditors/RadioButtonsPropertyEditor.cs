// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// A property editor to allow the individual selection of pre-defined items.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.RadioButtonList,
        "Radio button list",
        "radiobuttons",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-target")]
    public class RadioButtonsPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RadioButtonsPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
            _localizedTextService = localizedTextService;
        }

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_localizedTextService, _ioHelper);
    }
}
