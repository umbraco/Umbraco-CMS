using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a validator which ensures that all values in the list are unique.
    /// </summary>
    internal class ValueListUniqueValueValidator : IValueValidator
    {
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            // the value we get should be a JArray
            // [ { "value": <value>, "sortOrder": 1 }, { ... }, ... ]

            if (!(value is JArray json)) yield break;

            // we ensure that values are unique
            // (those are are not empty - empty values are removed when persisting anyways)

            var groupedValues = json.OfType<JObject>()
                .Where(x => x["value"] != null)
                .Select((x, index) => new { value = x["value"].ToString(), index})
                .Where(x => x.value.IsNullOrWhiteSpace() == false)
                .GroupBy(x => x.value);

            foreach (var group in groupedValues.Where(x => x.Count() > 1))
            {
                yield return new ValidationResult($"The value \"{@group.Last().value}\" must be unique", new[]
                {
                    // use the index number as server field so it can be wired up to the view
                    "item_" + @group.Last().index.ToInvariantString()
                });
            }
        }
    }
}