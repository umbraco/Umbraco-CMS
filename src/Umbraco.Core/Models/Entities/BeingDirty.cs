namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Provides a concrete implementation of <see cref="BeingDirtyBase" />.
/// </summary>
/// <remarks>
///     <para>
///         This class is provided for classes that cannot inherit from <see cref="BeingDirtyBase" />
///         and therefore need to implement <see cref="IRememberBeingDirty" />, by re-using some of
///         <see cref="BeingDirtyBase" /> logic.
///     </para>
/// </remarks>
public sealed class BeingDirty : BeingDirtyBase
{
    /// <summary>
    ///     Sets a property value, detects changes and manages the dirty flag.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The new value.</param>
    /// <param name="valueRef">A reference to the value to set.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="comparer">A comparer to compare property values.</param>
    public new void SetPropertyValueAndDetectChanges<T>(T value, ref T? valueRef, string propertyName, IEqualityComparer<T>? comparer = null) =>
        base.SetPropertyValueAndDetectChanges(value, ref valueRef, propertyName, comparer);

    /// <summary>
    ///     Registers that a property has changed.
    /// </summary>
    public new void OnPropertyChanged(string propertyName) => base.OnPropertyChanged(propertyName);
}
