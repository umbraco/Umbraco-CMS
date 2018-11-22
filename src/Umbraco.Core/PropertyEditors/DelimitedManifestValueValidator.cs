using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// A validator that validates a delimited set of values against a common regex
    /// </summary>
    [ValueValidator("Delimited")]
    internal sealed class DelimitedManifestValueValidator : ManifestValueValidator
    {
        /// <summary>
        /// Performs the validation
        /// </summary>
        /// <param name="value"></param>
        /// <param name="config">Can be a json formatted string containing properties: 'delimiter' and 'pattern'</param>
        /// <param name="preValues">The current pre-values stored for the data type</param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
        {
            //TODO: localize these!
            if (value != null)
            {
                var delimiter = ",";
                Regex regex = null;
                if (config.IsNullOrWhiteSpace() == false)
                {
                    var json = JsonConvert.DeserializeObject<JObject>(config);
                    if (json["delimiter"] != null)
                    {
                        delimiter = json["delimiter"].ToString();
                    }
                    if (json["pattern"] != null)
                    {
                        var regexPattern = json["pattern"].ToString();
                        regex = new Regex(regexPattern);
                    }
                }

                var stringVal = value.ToString();
                var split = stringVal.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < split.Length; i++)
                {
                    var s = split[i];
                    //next if we have a regex statement validate with that
                    if (regex != null)
                    {
                        if (regex.IsMatch(s) == false)
                        {
                            yield return new ValidationResult("The item at index " + i + " did not match the expression " + regex,
                                new[]
                                {
                                    //make the field name called 'value0' where 0 is the index
                                    "value" + i
                                });
                        }
                    }
                }
            }

        }
    }
}