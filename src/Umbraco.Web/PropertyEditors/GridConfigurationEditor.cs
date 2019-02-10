using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the grid value editor.
    /// </summary>
    public class GridConfigurationEditor : ConfigurationEditor<GridConfiguration>
    {
        public GridConfigurationEditor()
        {
            var items = Fields.First(x => x.Key == "items");

            items.Validators.Add(new GridValidator());
        }
    }

    public class GridValidator : IValueValidator
    {
        public IEnumerable<ValidationResult> Validate(object rawValue, string valueType, object dataTypeConfiguration)
        {
            if (rawValue == null)
                yield break;

            var model = JsonConvert.DeserializeObject<GridEditorModel>(rawValue.ToString());

            if (model.Templates.Any(t => t.Sections.Sum(s => s.Grid) > model.Columns))
            {
                yield return new ValidationResult("Columns must be at least the same size as the largest layout", new[] { nameof(model.Columns) });
            }

        }
    }

    public class GridEditorModel
    {
        public GridEditorTemplateModel[] Templates { get; set; }
        public int Columns { get; set; }
    }

    public class GridEditorTemplateModel
    {
        public GridEditorSectionModel[] Sections { get; set; }
    }

    public class GridEditorSectionModel
    {
        public int Grid { get; set; }
    }
}
