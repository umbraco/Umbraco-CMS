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
    /// A validator that validates that the value against a Regex expression
    /// </summary>
    [ValueValidator("Regex")]
    internal sealed class RegexValidator : ManifestValueValidator, IPropertyValidator
    {
        private readonly string _regex;

        /// <summary>
        /// Normally used when configured as a ManifestValueValidator
        /// </summary>
        public RegexValidator()
        {
        }

        /// <summary>
        /// Normally used when configured as an IPropertyValidator
        /// </summary>
        /// <param name="regex"></param>
        public RegexValidator(string regex)
        {
            if (regex == null) throw new ArgumentNullException("regex");
            _regex = regex;
        }

        public override IEnumerable<ValidationResult> Validate(object value, string config, PreValueCollection preValues, PropertyEditor editor)
        {
            string pattern = null;
            string customErrorMessage = null;

			//TODO: This would ideally be a JObject or plain object
            if (config.IsNullOrWhiteSpace() == false)
            {
                var json = JsonConvert.DeserializeObject<JObject>(config);
                if (json["pattern"] != null)
                {
                    pattern = json["pattern"].ToString();
                }
                if (json["customErrorMessage"] != null)
                {
                    customErrorMessage = json["customErrorMessage"].ToString();
                }
            }

            //TODO: localize these!
            if (pattern.IsNullOrWhiteSpace() == false && value != null)
            {
                var asString = value.ToString();

                var regex = new Regex(pattern);

                if (regex.IsMatch(asString) == false)
                {
                    yield return new ValidationResult(customErrorMessage ?? "Value is invalid, it does not match the correct pattern", new[] { "value" });
                }                
            }
            
        }

        /// <summary>
        /// Used when configured as an IPropertyValidator
        /// </summary>
        /// <param name="value"></param>
        /// <param name="preValues"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate(object value, PreValueCollection preValues, PropertyEditor editor)
        {
            if (_regex == null)
            {
                throw new InvalidOperationException("This validator is not configured as a " + typeof(IPropertyValidator));
            }
            return Validate(value, _regex, preValues, editor);
        }
    }
}