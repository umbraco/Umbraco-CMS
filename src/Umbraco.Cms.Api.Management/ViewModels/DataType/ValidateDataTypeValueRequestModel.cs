namespace Umbraco.Cms.Api.Management.ViewModels.DataType;

/// <summary>
/// Request model for validating a value against a data type's schema.
/// </summary>
public class ValidateDataTypeValueRequestModel
{
    /// <summary>
    /// The value to validate. Can be any JSON-compatible value (object, array, string, number, boolean, or null).
    /// </summary>
    public object? Value { get; set; }
}
