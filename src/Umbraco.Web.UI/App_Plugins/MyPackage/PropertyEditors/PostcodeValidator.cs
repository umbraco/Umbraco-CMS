using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    /// <summary>
    /// Validates a postcode
    /// </summary>
    internal class PostcodeValidator : IPropertyValidator
    {

        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            if (value != null)
            {
                var stringVal = value.ToString();

                if (preValues == null) yield break;
                var preValDicionary = preValues.FormatAsDictionary();
                if (preValDicionary.Any() == false) yield break;
                var asJson = JObject.Parse(preValDicionary.First().Value.ToString());
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