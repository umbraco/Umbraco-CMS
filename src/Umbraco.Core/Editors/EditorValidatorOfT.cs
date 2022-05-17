using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Editors;

/// <summary>
///     Provides a base class for <see cref="IEditorValidator" /> implementations.
/// </summary>
/// <typeparam name="T">The validated object type.</typeparam>
public abstract class EditorValidator<T> : IEditorValidator
{
    public Type ModelType => typeof(T);

    public IEnumerable<ValidationResult> Validate(object model) => Validate((T)model);

    protected abstract IEnumerable<ValidationResult> Validate(T model);
}
