using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence.Mappers;

/// <summary>
/// Defines a collection of mapper instances used for mapping between database entities and domain models in persistence operations.
/// </summary>
public interface IMapperCollection : IBuilderCollection<BaseMapper>
{
    /// <summary>
    /// Gets the <see cref="BaseMapper"/> instance associated with the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which to retrieve the mapper.</param>
    /// <returns>The <see cref="BaseMapper"/> instance corresponding to the specified type.</returns>
    BaseMapper this[Type type] { get; }

    /// <summary>
    /// Attempts to retrieve the mapper associated with the specified type.
    /// </summary>
    /// <param name="type">The type for which to retrieve the mapper.</param>
    /// <param name="mapper">When this method returns, contains the mapper associated with <paramref name="type"/> if found; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if a mapper for the specified type was found; otherwise, <c>false</c>.</returns>
    bool TryGetMapper(Type type, [MaybeNullWhen(false)] out BaseMapper mapper);
}
