using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     A validator that validates a delimited set of values against a common regex
/// </summary>
public sealed class DelimitedValueValidator : IManifestValueValidator
{
    /// <summary>
    ///     Gets or sets the configuration, when parsed as <see cref="IManifestValueValidator" />.
    /// </summary>
    public DelimitedValueValidatorConfig? Configuration { get; set; }

    /// <inheritdoc />
    public string ValidationName => "Delimited";

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
    {
        // TODO: localize these!
        if (value != null)
        {
            var delimiter = Configuration?.Delimiter ?? ",";
            Regex? regex = Configuration?.Pattern != null ? new Regex(Configuration.Pattern) : null;

            var stringVal = value.ToString();
            var split = stringVal!.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < split.Length; i++)
            {
                var s = split[i];

                // next if we have a regex statement validate with that
                if (regex != null)
                {
                    if (regex.IsMatch(s) == false)
                    {
                        yield return new ValidationResult(
                            "The item at index " + i + " did not match the expression " + regex,
                            new[]
                            {
                                // make the field name called 'value0' where 0 is the index
                                "value" + i,
                            });
                    }
                }
            }
        }
    }
}

public class DelimitedValueValidatorConfig
{
    public string? Delimiter { get; set; }

    public string? Pattern { get; set; }
}
