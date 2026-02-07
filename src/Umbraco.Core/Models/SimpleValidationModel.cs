namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a simple validation result model containing validation errors.
/// </summary>
public class SimpleValidationModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleValidationModel" /> class.
    /// </summary>
    /// <param name="modelState">A dictionary containing the validation errors keyed by property name.</param>
    /// <param name="message">The overall validation message. Defaults to "The request is invalid."</param>
    public SimpleValidationModel(IDictionary<string, object> modelState, string message = "The request is invalid.")
    {
        Message = message;
        ModelState = modelState;
    }

    /// <summary>
    /// Gets the overall validation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the dictionary containing validation errors keyed by property name.
    /// </summary>
    public IDictionary<string, object> ModelState { get; }
}
