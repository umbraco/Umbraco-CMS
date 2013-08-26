using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    /// <summary>
    /// Validates a postcode
    /// </summary>
    internal class PostcodeValidator : ValidatorBase
    {
        
        public override IEnumerable<ValidationResult> Validate(object value, string preValues, PropertyEditor editor)
        {
            if (value != null)
            {
                var stringVal = value.ToString();

                if (preValues.IsNullOrWhiteSpace()) yield break;
                var asJson = JObject.Parse(preValues);
                if (asJson["country"] == null) yield break;

                if (asJson["country"].ToString() == "Australia")
                {
                    if (Regex.IsMatch(stringVal, "^\\d{4}$") == false)
                    {
                        yield return new ValidationResult("Australian postcodes must be a 4 digit number",
                            new[]
                            {
                                //we only store a single value for this editor so the 'member' or 'field' 
                                // we'll associate this error with will simply be called 'value'
                                "value" 
                            });
                    }
                }
                else
                {
                    yield return new ValidationResult("Only Australian postcodes are supported for this validator");
                }
            }
            
        }
    }
}