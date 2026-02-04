using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     A factory for creating <see cref="IDataValueEditor" /> instances.
/// </summary>
public interface IDataValueEditorFactory
{
    /// <summary>
    ///     Creates a new instance of the specified <typeparamref name="TDataValueEditor" /> type.
    /// </summary>
    /// <typeparam name="TDataValueEditor">The type of data value editor to create.</typeparam>
    /// <param name="args">The constructor arguments for the data value editor.</param>
    /// <returns>A new instance of the specified data value editor type.</returns>
    TDataValueEditor Create<TDataValueEditor>(params object[] args)
        where TDataValueEditor : class, IDataValueEditor;
}
