using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is of a certain type
    /// </summary>
    /// <remarks>
    /// This is a special validator type that is executed against all property editors no matter if they've defined this validator or not.
    /// </remarks>
    [ValueValidator("ValueType")]
    internal sealed class ValueTypeValueValidator : ValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(string value, string config, string preValues, PropertyEditor editor)
        {
            var attempt = editor.ValueEditor.TryConvertValueToCrlType(value);
            if (attempt.Success == false)
            {
                //TODO: localize these!
                yield return new ValidationResult(string.Format("Value is not of type {0} and cannot be converted", editor.ValueEditor.GetDatabaseType()));
            }            
        }
    }
}