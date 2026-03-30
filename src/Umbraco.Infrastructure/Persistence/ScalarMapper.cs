namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides functionality to map scalar values of type <typeparamref name="T"/> between the application and the database.
/// </summary>
public abstract class ScalarMapper<T> : IScalarMapper
{
    /// <inheritdoc />
    object IScalarMapper.Map(object value) => Map(value)!;

    /// <summary>
    ///     Performs a strongly typed mapping operation for a scalar value.
    /// </summary>
    protected abstract T Map(object value);
}
