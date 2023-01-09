// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class DropDownFlexibleConfigurationEditor : ConfigurationEditor<DropDownFlexibleConfiguration>
{
    public DropDownFlexibleConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");

        // customize the items field
        items.Name = textService.Localize("editdatatype", "addPrevalue");
        items.Validators.Add(new ValueListUniqueValueValidator());
    }
}
