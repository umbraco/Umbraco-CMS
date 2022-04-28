// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        "Multinode Treepicker",
        "contentpicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        public MultiNodeTreePickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MultiNodeTreePickerPropertyValueEditor>(Attribute!);

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultiNodeTreePickerPropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {

            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
            {
                var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

                var udiPaths = asString!.Split(',');
                foreach (var udiPath in udiPaths)
                    if (UdiParser.TryParse(udiPath, out var udi))
                        yield return new UmbracoEntityReference(udi);
            }
        }
    }


}
