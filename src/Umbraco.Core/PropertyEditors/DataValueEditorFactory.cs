using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DataValueEditorFactory : IDataValueEditorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DataValueEditorFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public TDataValueEditor Create<TDataValueEditor>(params object[] args)
        where TDataValueEditor : class, IDataValueEditor
        => _serviceProvider.CreateInstance<TDataValueEditor>(args);
}
