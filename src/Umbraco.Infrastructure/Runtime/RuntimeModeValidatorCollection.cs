using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <summary>
/// Represents a collection that manages <see cref="IRuntimeModeValidator"/> instances used to validate the application's runtime mode.
/// </summary>
public class RuntimeModeValidatorCollection : BuilderCollectionBase<IRuntimeModeValidator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeModeValidatorCollection"/> class with the specified function to retrieve <see cref="IRuntimeModeValidator"/> instances.
    /// </summary>
    /// <param name="items">A function that returns an <see cref="IEnumerable{IRuntimeModeValidator}"/>.</param>
    public RuntimeModeValidatorCollection(Func<IEnumerable<IRuntimeModeValidator>> items)
        : base(items)
    { }
}
