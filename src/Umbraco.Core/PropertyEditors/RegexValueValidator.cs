using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value against a Regex expression
    /// </summary>
    [ValueValidator("Regex")]
    internal sealed class RegexValueValidator : ValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!
            if (config.IsNullOrWhiteSpace() == false && value != null)
            {
                var asString = value.ToString();

                var regex = new Regex(config);

                if (regex.IsMatch(asString) == false)
                {
                    yield return new ValidationResult("Value is invalid, it does not match the correct pattern", new[] { "value" });
                }                
            }
            
        }
    }
}