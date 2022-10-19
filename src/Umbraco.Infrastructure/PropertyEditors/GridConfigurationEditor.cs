// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration editor for the grid value editor.
/// </summary>
public class GridConfigurationEditor : ConfigurationEditor<GridConfiguration>
{
    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public GridConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public GridConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(ioHelper, editorConfigurationParser)
    {
        ConfigurationField items = Fields.First(x => x.Key == "items");

        items.Validators.Add(new GridValidator());
    }
}

public class GridValidator : IValueValidator
{
    public IEnumerable<ValidationResult> Validate(object? rawValue, string? valueType, object? dataTypeConfiguration)
    {
        if (rawValue == null)
        {
            yield break;
        }

        GridEditorModel? model = JsonConvert.DeserializeObject<GridEditorModel>(rawValue.ToString()!);

        if (model?.Templates?.Any(t => t.Sections?.Sum(s => s.Grid) > model.Columns) ?? false)
        {
            yield return new ValidationResult(
                "Columns must be at least the same size as the largest layout",
                new[] { nameof(model.Columns) });
        }
    }
}

public class GridEditorModel
{
    public GridEditorTemplateModel[]? Templates { get; set; }

    public int Columns { get; set; }
}

public class GridEditorTemplateModel
{
    public GridEditorSectionModel[]? Sections { get; set; }
}

public class GridEditorSectionModel
{
    public int Grid { get; set; }
}
