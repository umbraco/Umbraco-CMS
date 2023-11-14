using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
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


    /// <summary>
    ///  Constructor for the JsonPropertyIndexValueFactoryBase.
    /// </summary>
    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    protected JsonPropertyIndexValueFactoryBase(IJsonSerializer jsonSerializer): this(jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {

    }

    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var result = new List<KeyValuePair<string, IEnumerable<object?>>>();

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

        IEnumerable<KeyValuePair<string, IEnumerable<object?>>> summary = HandleResume(result, property, culture, segment, published);
        if (_indexingSettings.ExplicitlyIndexEachNestedProperty || ForceExplicitlyIndexEachNestedProperty)
        {
            result.AddRange(summary);
            return result;
        }

        return summary;
    }

    /// <inheritdoc />
    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures)
    => GetIndexValues(
        property,
        culture,
        segment,
        published,
        Enumerable.Empty<string>(),
        StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>().GetAll().ToDictionary(x=>x.Key));

    [Obsolete("Use method overload that has availableCultures, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        => GetIndexValues(
            property,
            culture,
            segment,
            published,
            Enumerable.Empty<string>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>().GetAll().ToDictionary(x=>x.Key));

    /// <summary>
    ///  Method to return a list of summary of the content. By default this returns an empty list
    /// </summary>
    protected virtual IEnumerable<KeyValuePair<string, IEnumerable<object?>>> HandleResume(
        List<KeyValuePair<string, IEnumerable<object?>>> result,
        IProperty property,
        string? culture,
        string? segment,
        bool published) => Array.Empty<KeyValuePair<string, IEnumerable<object?>>>();

    /// <summary>
    ///  Method that handle the deserialized object.
    /// </summary>
    [Obsolete("Use the non-obsolete overload instead, scheduled for removal in v14")]
    protected abstract IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published);

    [Obsolete("Use the non-obsolete overload instead, scheduled for removal in v14")]
    protected virtual IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures) => Handle(deserializedPropertyValue, property, culture, segment, published);

    /// <summary>
    ///  Method that handle the deserialized object.
    /// </summary>
    protected virtual IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
        => Handle(deserializedPropertyValue, property, culture, segment, published, availableCultures);
}
