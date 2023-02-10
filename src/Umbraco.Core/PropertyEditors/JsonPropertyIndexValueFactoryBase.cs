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

        var val = property.GetValue(culture, segment, published);

        //if there is a value, it's a string and it's detected as json
        if (val is string rawVal && rawVal.DetectIsJson())
        {
            try
            {
                TSerialized? deserializedObject = _jsonSerializer.Deserialize<TSerialized>(rawVal);

                if (deserializedObject is null)
                {
                    return result;
                }

                result.AddRange(Handle(deserializedObject, property, culture, segment, published));
            }
            catch (InvalidCastException)
            {
                //swallow...on purpose, there's a chance that this isn't the json format we are looking for
                // and we don't want that to affect the website.
            }
            catch (ArgumentException)
            {
                //swallow on purpose to prevent this error:
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
        TSerialized deserializedObject,
        IProperty property,
        string? culture,
        string? segment,
        bool published);
}
