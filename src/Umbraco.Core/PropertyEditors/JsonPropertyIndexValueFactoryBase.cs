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

    /// <summary>
    ///  Constructor for the JsonPropertyIndexValueFactoryBase.
    /// </summary>
    protected JsonPropertyIndexValueFactoryBase(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published)
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

                result.AddRange(Handle(deserializedPropertyValue, property, culture, segment, published));
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

        result.AddRange(HandleResume(result, property, culture, segment, published));

        return result;
    }

    /// <summary>
    ///  Method to return a list of resume of the content. By default this returns an empty list
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
    protected abstract IEnumerable<KeyValuePair<string, IEnumerable<object?>>> Handle(
        TSerialized deserializedPropertyValue,
        IProperty property,
        string? culture,
        string? segment,
        bool published);
}
