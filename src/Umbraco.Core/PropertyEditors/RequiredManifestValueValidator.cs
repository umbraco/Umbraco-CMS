using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is not null or empty (if it is a string)
    /// </summary>
    [ValueValidator("Required")]
    internal sealed class RequiredManifestValueValidator : ManifestValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
        {
            //TODO: localize these!

            if (value == null)
            {
                yield return new ValidationResult("Value cannot be null", new[] {"value"});
            }
            else
            {
                var asString = value.ToString();

                if (editor.ValueEditor.ValueType.InvariantEquals("JSON"))
                {
                    if (asString.DetectIsEmptyJson())
                    {
                        yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                    }
                }

                if (asString.IsNullOrWhiteSpace())
                {
                    yield return new ValidationResult("Value cannot be empty", new[] { "value" });
                }
            }

            
        }
    }
}