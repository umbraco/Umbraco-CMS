using System.Diagnostics.CodeAnalysis;

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

    /// <summary>
    /// Attempts to parse an object that represents a JSON structure - i.e. a JSON object or a JSON array - to a strongly typed representation.
    /// </summary>
    /// <typeparam name="T">The target type of the JSON value.</typeparam>
    /// <param name="input">The object input to parse.</param>
    /// <param name="value">The parsed result, or null if the parsing fails.</param>
    /// <returns>True if the parsing results in a non-null value, false otherwise.</returns>
    bool TryDeserialize<T>(object input, [NotNullWhen(true)] out T? value)
        where T : class;
}
