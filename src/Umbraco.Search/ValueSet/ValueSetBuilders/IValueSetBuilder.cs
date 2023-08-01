using Examine;

namespace Umbraco.Search.ValueSet.ValueSetBuilders;

/// <summary>
///     Creates a collection of <see cref="ValueSet" /> to be indexed based on a collection of <see cref="T" />
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IValueSetBuilder<in T> : IValueSetBuilder
{
    /// <summary>
    ///     Creates a collection of <see cref="ValueSet" /> to be indexed based on a collection of <see cref="T" />
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    IEnumerable<UmbracoValueSet> GetValueSets(params T[] content);
}

public interface IValueSetBuilder
{

}
