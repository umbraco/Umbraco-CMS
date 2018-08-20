namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods for System.Decimal.
    /// </summary>
    /// <remarks>See System.Decimal on MSDN and also 
    /// http://stackoverflow.com/questions/4298719/parse-decimal-and-filter-extra-0-on-the-right/4298787#4298787.
    /// </remarks>
    public static class DecimalExtensions
    {
        /// <summary>
        /// Gets the normalized value.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <returns>The normalized value.</returns>
        /// <remarks>Normalizing changes the scaling factor and removes trailing zeroes,
        /// so 1.2500m comes out as 1.25m.</remarks>
        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }
    }
}
