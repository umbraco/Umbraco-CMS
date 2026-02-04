using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a factory for creating <see cref="IDataValueEditor"/> instances.
/// </summary>
public class DataValueEditorFactory : IDataValueEditorFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataValueEditorFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    public DataValueEditorFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public TDataValueEditor Create<TDataValueEditor>(params object[] args)
        where TDataValueEditor : class, IDataValueEditor
        => _serviceProvider.CreateInstance<TDataValueEditor>(args);
}
