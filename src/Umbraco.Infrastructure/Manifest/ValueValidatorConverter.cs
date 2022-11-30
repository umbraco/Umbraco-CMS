using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Implements a json read converter for <see cref="IValueValidator" />.
/// </summary>
internal class ValueValidatorConverter : JsonReadConverter<IValueValidator>
{
    private readonly ManifestValueValidatorCollection _validators;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueValidatorConverter" /> class.
    /// </summary>
    public ValueValidatorConverter(ManifestValueValidatorCollection validators) => _validators = validators;

    protected override IValueValidator Create(Type objectType, string path, JObject jObject)
    {
        var type = jObject["type"]?.Value<string>();
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new InvalidOperationException("Could not get the type of the validator.");
        }

        return _validators.GetByName(type);

        // jObject["configuration"] is going to be deserialized in a Configuration property, if any
    }
}
