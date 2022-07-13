namespace Umbraco.Cms.Core.Models;

public class SimpleValidationModel
{
    public SimpleValidationModel(IDictionary<string, object> modelState, string message = "The request is invalid.")
    {
        Message = message;
        ModelState = modelState;
    }

    public string Message { get; }

    public IDictionary<string, object> ModelState { get; }
}
