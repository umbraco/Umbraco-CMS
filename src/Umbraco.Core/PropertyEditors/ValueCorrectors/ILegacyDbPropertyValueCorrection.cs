namespace Umbraco.Core.PropertyEditors.ValueCorrectors
{
    /// <summary>
    /// Defines a database property value correction.
    /// </summary>
    /// <remarks>
    /// Used for changing legacy database stored values into new formats
    /// </remarks>
    internal interface ILegacyDbPropertyValueCorrection
    {
        /// <summary>
        /// Corrects (overrides) the object value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The corrected value.
        /// </returns>
        object ApplyCorrection(object value);
    }
}