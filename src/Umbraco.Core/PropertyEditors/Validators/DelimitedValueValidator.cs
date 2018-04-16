using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.PropertyEditors.Validators
{
    /// <summary>
    /// A validator that validates a delimited set of values against a common regex
    /// </summary>
    internal sealed class DelimitedValueValidator : IManifestValueValidator
    {
        /// <inheritdoc />
        public string ValidationName => "Delimited";

        /// <summary>
        /// Gets or sets the configuration, when parsed as <see cref="IManifestValueValidator"/>.
        /// </summary>
        public JObject Configuration { get; set; }

        /// <inheritdoc />
        public IEnumerable<ValidationResult> Validate(object value, string valueType, object dataTypeConfiguration)
        {
            //TODO: localize these!
            if (value != null)
            {
                var delimiter = ",";
                Regex regex = null;
                if (Configuration is JObject jobject)
                {
                    if (jobject["delimiter"] != null)
                    {
                        delimiter = jobject["delimiter"].ToString();
                    }
                    if (jobject["pattern"] != null)
                    {
                        var regexPattern = jobject["pattern"].ToString();
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
