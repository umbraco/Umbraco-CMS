using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Creates a collection of <see cref="ValueSet" /> to be indexed based on a collection of <see cref="T" />
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IValueSetBuilder<in T>
{
    /// <summary>
    ///     Creates a collection of <see cref="ValueSet" /> to be indexed based on a collection of <see cref="T" />
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    IEnumerable<ValueSet> GetValueSets(params T[] content);
}
