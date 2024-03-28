namespace Umbraco.Cms.Core.Serialization;

/// <summary>
/// Provides functionality to serialize objects or value types to JSON and to deserialize JSON into objects or value types.
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Converts the specified <paramref name="input" /> into a JSON string.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <returns>
    /// A JSON string representation of the value.
    /// </returns>
    string Serialize(object? input);


    /// <summary>
    /// Parses the text representing a single JSON value into an instance of the type specified by a generic type parameter.
    /// </summary>
    /// <typeparam name="T">The target type of the JSON value.</typeparam>
    /// <param name="input">The JSON input to parse.</param>
    /// <returns>
    /// A <typeparamref name="T" /> representation of the JSON value.
    /// </returns>
    T? Deserialize<T>(string input);
}
