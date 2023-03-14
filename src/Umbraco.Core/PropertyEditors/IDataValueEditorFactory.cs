using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

public interface IDataValueEditorFactory
{
    TDataValueEditor Create<TDataValueEditor>(params object[] args)
        where TDataValueEditor : class, IDataValueEditor;
}
