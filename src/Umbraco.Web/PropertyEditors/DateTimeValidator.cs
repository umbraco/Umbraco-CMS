using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Used to validate if the value is a valid date/time
    /// </summary>
    internal class DateTimeValidator : ValidatorBase
    {        
        public override IEnumerable<ValidationResult> Validate(string value, string preValues, PropertyEditor editor)
        {
            DateTime dt;
            if (value.IsNullOrWhiteSpace() == false &&  DateTime.TryParse(value, out dt) == false)
            {
                yield return new ValidationResult(string.Format("The string value {0} cannot be parsed into a DateTime", value),
                                                  new[]
                                                      {
                                                          //we only store a single value for this editor so the 'member' or 'field' 
                                                          // we'll associate this error with will simply be called 'value'
                                                          "value" 
                                                      });
            }
        }
    }
}