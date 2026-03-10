using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides index value factory for tag properties.
/// </summary>
public class TagPropertyIndexValueFactory : JsonPropertyIndexValueFactoryBase<string[]>, ITagPropertyIndexValueFactory
{
    private IndexingSettings _indexingSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagPropertyIndexValueFactory"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="indexingSettings">The indexing settings.</param>
    public TagPropertyIndexValueFactory(
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(jsonSerializer, indexingSettings)
    {
        ForceExplicitlyIndexEachNestedProperty = true;
        _indexingSettings = indexingSettings.CurrentValue;
        indexingSettings.OnChange(newValue => _indexingSettings = newValue);
    }

    /// <inheritdoc />
    protected override IEnumerable<IndexValue> Handle(
        string[] deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
        =>
        [
            new IndexValue
            {
                Culture = culture,
                FieldName = property.Alias,
                Values = deserializedPropertyValue
            }
        ];

    /// <inheritdoc />
    public override IEnumerable<IndexValue> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        IEnumerable<IndexValue> jsonValues = base.GetIndexValues(property, culture, segment, published, availableCultures, contentTypeDictionary);
        if (jsonValues?.Any() is true)
        {
            return jsonValues;
        }

        var result = new List<IndexValue>();

        var propertyValue = property.GetValue(culture, segment, published);

        // If there is a value, it's a string and it's not empty/white space
        if (propertyValue is string rawValue && !string.IsNullOrWhiteSpace(rawValue))
        {
            var values = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            result.AddRange(Handle(values, property, culture, segment, published, availableCultures, contentTypeDictionary));
        }

        IEnumerable<IndexValue> summary = HandleResume(result, property, culture, segment, published);
        if (_indexingSettings.ExplicitlyIndexEachNestedProperty || ForceExplicitlyIndexEachNestedProperty)
        {
            result.AddRange(summary);
            return result;
        }

        return summary;
    }
}
