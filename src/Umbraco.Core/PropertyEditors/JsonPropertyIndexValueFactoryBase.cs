using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///  Abstract base for property index value factories where the value is json.
/// </summary>
/// <typeparam name="TSerialized">The type to deserialize the json to.</typeparam>
public abstract class JsonPropertyIndexValueFactoryBase<TSerialized> : IPropertyIndexValueFactory
{
    private readonly IJsonSerializer _jsonSerializer;
    private IndexingSettings _indexingSettings;

    protected bool ForceExplicitlyIndexEachNestedProperty { get; set; }

    /// <summary>
    ///  Constructor for the JsonPropertyIndexValueFactoryBase.
    /// </summary>
    protected JsonPropertyIndexValueFactoryBase(IJsonSerializer jsonSerializer, IOptionsMonitor<IndexingSettings> indexingSettings)
    {
        _jsonSerializer = jsonSerializer;
        _indexingSettings = indexingSettings.CurrentValue;
        indexingSettings.OnChange(newValue => _indexingSettings = newValue);
    }

    public virtual IEnumerable<IndexValue> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var result = new List<IndexValue>();

        var propertyValue = property.GetValue(culture, segment, published);

        // If there is a value, it's a string and it's detected as json.
        if (propertyValue is string rawValue && rawValue.DetectIsJson())
        {
            try
            {
                TSerialized? deserializedPropertyValue = _jsonSerializer.Deserialize<TSerialized>(rawValue);

                if (deserializedPropertyValue is null)
                {
                    return result;
                }

                result.AddRange(Handle(deserializedPropertyValue, property, culture, segment, published, availableCultures, contentTypeDictionary));
            }
            catch (InvalidCastException)
            {
                // Swallow...on purpose, there's a chance that this isn't the json format we are looking for
                // and we don't want that to affect the website.
            }
            catch (ArgumentException)
            {
                // Swallow on purpose to prevent this error:
                // Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.
            }
        }

        IEnumerable<IndexValue> summary = HandleResume(result, property, culture, segment, published);
        if (_indexingSettings.ExplicitlyIndexEachNestedProperty || ForceExplicitlyIndexEachNestedProperty)
        {
            result.AddRange(summary);
            return result;
        }

        return summary;
    }

    /// <summary>
    ///  Method to return a list of summary of the content. By default this returns an empty list
    /// </summary>
    protected virtual IEnumerable<IndexValue> HandleResume(
        List<IndexValue> result,
        IProperty property,
        string? culture,
        string? segment,
        bool published) => Array.Empty<IndexValue>();

    /// <summary>
    ///  Method that handle the deserialized object.
    /// </summary>
    protected abstract IEnumerable<IndexValue> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary);
}
