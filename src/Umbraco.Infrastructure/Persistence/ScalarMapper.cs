using System;

namespace Umbraco.Cms.Infrastructure.Persistence;

public abstract class ScalarMapper<T> : IScalarMapper
{
    /// <summary>
    /// Performs a strongly typed mapping operation for a scalar value.
    /// </summary>
    protected abstract T Map(object value);

    /// <inheritdoc />
    object IScalarMapper.Map(object value) => Map(value);
}
