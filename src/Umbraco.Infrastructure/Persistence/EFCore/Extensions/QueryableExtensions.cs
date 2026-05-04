namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Extensions;

/// <summary>
///     Introduces Queryable extensions to EFCore. This is where our custom queries live.
/// </summary>
public static class QueryableExtensions
{
    extension<T>(IQueryable<T> source)
    {
        /// <summary>
        ///     Bypasses a specified number of elements, supporting offsets larger than <see cref="int.MaxValue"/>.
        ///     The intended use of this is to use in case a method uses <see cref="long"/> as a parameter for skip.
        /// </summary>
        public IQueryable<T> BigSkip(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Skip count must be non-negative.");
            }

            if (count <= int.MaxValue)
            {
                return source.Skip((int)count);
            }

            IQueryable<T> result = source;
            long remaining = count;
            while (remaining > int.MaxValue)
            {
                result = result.Skip(int.MaxValue);
                remaining -= int.MaxValue;
            }

            // Casting is safe here as we have ensured the variable is within the limits of int.
            return result.Skip((int)remaining);
        }
    }
}
