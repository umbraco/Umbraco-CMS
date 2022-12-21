// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Pre-value editor used to create a list of items
/// </summary>
/// <remarks>
///     This pre-value editor is shared with editors like drop down, checkbox list, etc....
/// </remarks>
public class ValueListConfigurationEditor : ConfigurationEditor<ValueListConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public ValueListConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper)
        : this(textService, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public ValueListConfigurationEditor(ILocalizedTextService textService, IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");

        // customize the items field
        items.Name = textService.Localize("editdatatype", "addPrevalue");
        items.Validators.Add(new ValueListUniqueValueValidator());
    }
}
