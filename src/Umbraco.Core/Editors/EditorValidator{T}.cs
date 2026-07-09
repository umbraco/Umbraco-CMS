using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Editors;

/// <summary>
/// Provides a base class for <see cref="IEditorValidator"/> implementations.
/// </summary>
/// <typeparam name="T">The validated object type.</typeparam>
public abstract class EditorValidator<T> : IEditorValidator
{
    /// <inheritdoc />
    public Type ModelType => typeof(T);

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(object model) => Validate((T)model);

    /// <summary>
    /// Validates the specified model instance.
    /// </summary>
    /// <param name="model">The strongly-typed model to validate.</param>
    /// <returns>A collection of validation results indicating any validation failures.</returns>
    protected abstract IEnumerable<ValidationResult> Validate(T model);
}
