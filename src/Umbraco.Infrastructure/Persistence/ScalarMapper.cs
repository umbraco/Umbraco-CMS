namespace Umbraco.Cms.Infrastructure.Persistence;

public abstract class ScalarMapper<T> : IScalarMapper
{
    /// <inheritdoc />
    object IScalarMapper.Map(object value) => Map(value)!;

    /// <summary>
    ///     Performs a strongly typed mapping operation for a scalar value.
    /// </summary>
    protected abstract T Map(object value);
}
