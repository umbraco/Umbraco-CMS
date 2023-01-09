using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Provides a base implementation of <see cref="ICanBeDirty" /> and <see cref="IRememberBeingDirty" />.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public abstract class BeingDirtyBase : IRememberBeingDirty
{
    private Dictionary<string, bool>? _currentChanges; // which properties have changed?
    private Dictionary<string, bool>? _savedChanges; // which properties had changed at last commit?
    private bool _withChanges = true; // should we track changes?

    #region ICanBeDirty

    /// <inheritdoc />
    public virtual bool IsDirty() => _currentChanges != null && _currentChanges.Any();

    /// <inheritdoc />
    public virtual bool IsPropertyDirty(string propertyName) =>
        _currentChanges != null && _currentChanges.ContainsKey(propertyName);

    /// <inheritdoc />
    public virtual IEnumerable<string> GetDirtyProperties() =>

        // ReSharper disable once MergeConditionalExpression
        _currentChanges == null
            ? Enumerable.Empty<string>()
            : _currentChanges.Where(x => x.Value).Select(x => x.Key);

    /// <inheritdoc />
    /// <remarks>Saves dirty properties so they can be checked with WasDirty.</remarks>
    public virtual void ResetDirtyProperties() => ResetDirtyProperties(true);

    #endregion

    #region IRememberBeingDirty

    /// <inheritdoc />
    public virtual bool WasDirty() => _savedChanges != null && _savedChanges.Any();

    /// <inheritdoc />
    public virtual bool WasPropertyDirty(string propertyName) =>
        _savedChanges != null && _savedChanges.ContainsKey(propertyName);

    /// <inheritdoc />
    public virtual void ResetWereDirtyProperties() =>

        // note: cannot .Clear() because when memberwise-cloning this will be the SAME
        // instance as the one on the clone, so we need to create a new instance.
        _savedChanges = null;

    /// <inheritdoc />
    public virtual void ResetDirtyProperties(bool rememberDirty)
    {
        // capture changes if remembering
        // clone the dictionary in case it's shared by an entity clone
        _savedChanges = rememberDirty && _currentChanges != null
            ? _currentChanges.ToDictionary(v => v.Key, v => v.Value)
            : null;

        // note: cannot .Clear() because when memberwise-clone this will be the SAME
        // instance as the one on the clone, so we need to create a new instance.
        _currentChanges = null;
    }

    /// <inheritdoc />
    public virtual IEnumerable<string> GetWereDirtyProperties() =>

        // ReSharper disable once MergeConditionalExpression
        _savedChanges == null
            ? Enumerable.Empty<string>()
            : _savedChanges.Where(x => x.Value).Select(x => x.Key);

    #endregion

    #region Change Tracking

    /// <summary>
    ///     Occurs when a property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void ClearPropertyChangedEvents() => PropertyChanged = null;

    /// <summary>
    ///     Registers that a property has changed.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (_withChanges == false)
        {
            return;
        }

        if (_currentChanges == null)
        {
            _currentChanges = new Dictionary<string, bool>();
        }

        _currentChanges[propertyName] = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Disables change tracking.
    /// </summary>
    public void DisableChangeTracking() => _withChanges = false;

    /// <summary>
    ///     Enables change tracking.
    /// </summary>
    public void EnableChangeTracking() => _withChanges = true;

    /// <summary>
    ///     Sets a property value, detects changes and manages the dirty flag.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The new value.</param>
    /// <param name="valueRef">A reference to the value to set.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="comparer">A comparer to compare property values.</param>
    protected void SetPropertyValueAndDetectChanges<T>(T? value, ref T? valueRef, string propertyName, IEqualityComparer<T>? comparer = null)
    {
        if (comparer == null)
        {
            // if no comparer is provided, use the default provider, as long as the value is not
            // an IEnumerable - exclude strings, which are IEnumerable but have a default comparer
            Type typeofT = typeof(T);
            if (!(typeofT == typeof(string)) && typeof(IEnumerable).IsAssignableFrom(typeofT))
            {
                throw new ArgumentNullException(nameof(comparer), "A custom comparer must be supplied for IEnumerable values.");
            }

            comparer = EqualityComparer<T>.Default;
        }

        // compare values
        var changed = _withChanges && comparer.Equals(valueRef, value) == false;

        // assign the new value
        valueRef = value;

        // handle change
        if (changed)
        {
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    ///     Detects changes and manages the dirty flag.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The new value.</param>
    /// <param name="orig">The original value.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="comparer">A comparer to compare property values.</param>
    /// <param name="changed">A value indicating whether we know values have changed and no comparison is required.</param>
    protected void DetectChanges<T>(T value, T orig, string propertyName, IEqualityComparer<T> comparer, bool changed)
    {
        // compare values
        changed = _withChanges && (changed || !comparer.Equals(orig, value));

        // handle change
        if (changed)
        {
            OnPropertyChanged(propertyName);
        }
    }

    #endregion
}
