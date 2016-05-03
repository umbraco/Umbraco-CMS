namespace Umbraco.Core.PropertyEditors.ValueCorrectors
{
    using System;
    using System.Linq;

    /// <summary>
    /// Overrides the stored database value for the repeatable text string editor.
    /// </summary>
    [DbPropertyValueCorrection("Umbraco.MultipleTextstring")]
    internal class RepeatableTextstringDbValueCorrection : DbPropertyValueCorrectionBase
    {
        /// <summary>
        /// Performs the actual work of applying the correction.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The corrected value as <see cref="object"/>.
        /// </returns>
        protected override object CorrectValue(object value)
        {
            if (EnsureValueNeedsCorrection(value) == false) return value;

            // split the value into multiple lines
            var lines = value.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);

            // join the lines as XML values
            return string.Join(
                string.Empty,
                lines.Select(x => string.Format("<value>{0}</value>", x)));
        }

        /// <summary>
        /// Ensure the value needs to be corrected.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool EnsureValueNeedsCorrection(object value)
        {
            var sourceString = value.ToString().Trim();
            return sourceString.IndexOf("<value>", StringComparison.Ordinal) != 0;
        }
    }
}