namespace Umbraco.Core.PropertyEditors.ValueCorrectors
{
    using System;

    /// <summary>
    /// A base class for <see cref="ILegacyDbPropertyValueCorrection"/>s.
    /// </summary>
    /// <remarks>
    /// Required for instantiation
    /// </remarks>
    internal abstract class DbPropertyValueCorrectionBase : ILegacyDbPropertyValueCorrection
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
        public object ApplyCorrection(object value)
        {
            try
            {
                return this.CorrectValue(value);
            }
            catch (Exception ex)
            {
                // TODO log with warning
                return value;
            }
        }

        /// <summary>
        /// Corrects (overrides) the object value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The corrected value.
        /// </returns>
        protected abstract object CorrectValue(object value);
    }
}