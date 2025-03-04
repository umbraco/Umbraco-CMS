namespace Umbraco.Cms.Core.PropertyEditors.Validation;

/// <summary>
/// Provides helper methods for validation of property editor values based on data type configuration.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Checks if a provided value is valid based on the configured step and minimum values.
    /// </summary>
    /// <param name="value">The provided value.</param>
    /// <param name="min">The configured minimum value.</param>
    /// <param name="step">The configured step value.</param>
    /// <returns>True if the value is valid otherwise false.</returns>
    public static bool IsValueValidForStep(decimal value, decimal min, decimal step)
    {
        if (value < min)
        {
            return true; // Outside of the range, so we expect another validator will have picked this up.
        }

        if (step == 0)
        {
            return true; // A step of zero would trigger a divide by zero error in evaluating. So we always pass validation for zero, as effectively any step value is valid.
        }

        return (value - min) % step == 0;
    }
}
