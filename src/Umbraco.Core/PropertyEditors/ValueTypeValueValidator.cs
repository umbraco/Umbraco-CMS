using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates that the value is of a certain type
    /// </summary>
    /// <remarks>
    /// This is a special validator type that is executed against all property editors no matter if they've defined this validator or not.
    /// </remarks>
    [ValueValidator("ValueType")]
    internal sealed class ValueTypeValueValidator : ManifestValueValidator
    {
        public override IEnumerable<ValidationResult> Validate(object value, string config, string preValues, PropertyEditor editor)
        {
            //TODO: localize these!

            Type valueType;
            //convert the string to a known type
            switch (editor.ValueEditor.ValueType.ToUpper())
            {
                case "INT":
                    valueType = typeof (int);
                    break;
                case "STRING":
                case "TEXT":
                    valueType = typeof (string);
                    break;
                case "DATETIME":
                case "DATE":
                case "TIME":
                    valueType = typeof(string);
                    break;
                default:
                    throw new FormatException("The valueType parameter does not match a known value type");
            }

            var attempt = value.TryConvertTo(valueType);
            if (!attempt.Success)
            {
                yield return new ValidationResult(string.Format("Value is not of type {0} and cannot be converted", valueType));
            }            
        }
    }
}