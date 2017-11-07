using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// A base class for use to implement IRememberBeingDirty/ICanBeDirty
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class TracksChangesEntityBase : IRememberBeingDirty
    {
        private bool _changeTrackingEnabled = true; // should we track changes?
        private IDictionary<string, bool> _propertyChangedInfo; // which properties have changed?
        private IDictionary<string, bool> _lastPropertyChangedInfo; // which properties had changed at last commit?

        /// <summary>
        /// Gets properties that are dirty.
        /// </summary>
        public virtual IEnumerable<string> GetDirtyProperties()
        {
            if (_propertyChangedInfo == null)
                return Enumerable.Empty<string>();

            return _propertyChangedInfo.Where(x => x.Value).Select(x => x.Key);
        }

        /// <summary>
        /// Occurs when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Registers that a property has changed.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            if (_changeTrackingEnabled == false)
                return;

            if (_propertyChangedInfo == null)
                _propertyChangedInfo = new Dictionary<string, bool>();

            _propertyChangedInfo[propertyInfo.Name] = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        /// <summary>
        /// Gets a value indicating whether a specific property is dirty.
        /// </summary>
        public virtual bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo != null && _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Gets a value indicating whether a specific property was dirty.
        /// </summary>
        /// <remarks>A property was dirty if it had been changed and the changes were committed.</remarks>
        public virtual bool WasPropertyDirty(string propertyName)
        {
            return _lastPropertyChangedInfo != null && _lastPropertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Gets a value indicating whether the current entity is dirty.
        /// </summary>
        public virtual bool IsDirty()
        {
            return _propertyChangedInfo != null && _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Gets a value indicating whether the current entity is dirty.
        /// </summary>
        /// <remarks>A property was dirty if it had been changed and the changes were committed.</remarks>
        public virtual bool WasDirty()
        {
            return _lastPropertyChangedInfo != null && _lastPropertyChangedInfo.Any();
        }

        /// <summary>
        /// Resets dirty properties.
        /// </summary>
        /// <remarks>Saves dirty properties so they can be checked with WasDirty.</remarks>
        public virtual void ResetDirtyProperties()
        {
            ResetDirtyProperties(true);
        }

        /// <summary>
        /// Resets dirty properties.
        /// </summary>
        /// <param name="rememberDirty">A value indicating whether to remember dirty properties.</param>
        /// <remarks>When <paramref name="rememberDirty"/> is true, dirty properties are saved so they can be checked with WasDirty.</remarks>
        public virtual void ResetDirtyProperties(bool rememberDirty)
        {
            if (rememberDirty && _propertyChangedInfo != null)
            {
                _lastPropertyChangedInfo = _propertyChangedInfo.ToDictionary(v => v.Key, v => v.Value);
            }

            // note: cannot .Clear() because when memberwise-clone this will be the SAME
            // instance as the one on the clone, so we need to create a new instance.
            _propertyChangedInfo = null;
        }

        /// <summary>
        /// Resets properties that were dirty.
        /// </summary>
        public void ResetWereDirtyProperties()
        {
            // note: cannot .Clear() because when memberwise-cloning this will be the SAME
            // instance as the one on the clone, so we need to create a new instance.
            _lastPropertyChangedInfo = null;
        }

        /// <summary>
        /// Resets all change tracking infos.
        /// </summary>
        public void ResetChangeTrackingCollections()
        {
            _propertyChangedInfo = null;
            _lastPropertyChangedInfo = null;
        }

        /// <summary>
        /// Disables change tracking.
        /// </summary>
        public void DisableChangeTracking()
        {
            _changeTrackingEnabled = false;
        }

        /// <summary>
        /// Enables change tracking.
        /// </summary>
        public void EnableChangeTracking()
        {
            _changeTrackingEnabled = true;
        }

        /// <summary>
        /// Sets a property value, detects changes and manages the dirty flag.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The new value.</param>
        /// <param name="valueRef">A reference to the value to set.</param>
        /// <param name="propertySelector">The property selector.</param>
        internal void SetPropertyValueAndDetectChanges<T>(T value, ref T valueRef, PropertyInfo propertySelector)
        {
            if ((typeof(T) == typeof(string) == false) && TypeHelper.IsTypeAssignableFrom<IEnumerable>(typeof(T)))
            {
                throw new InvalidOperationException("This method does not support IEnumerable instances. For IEnumerable instances a manual custom equality check will be required");
            }

            SetPropertyValueAndDetectChanges(value, ref valueRef, propertySelector, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Sets a property value, detects changes and manages the dirty flag.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The new value.</param>
        /// <param name="valueRef">A reference to the value to set.</param>
        /// <param name="propertySelector">The property selector.</param>
        /// <param name="comparer">A comparer to compare property values.</param>
        internal void SetPropertyValueAndDetectChanges<T>(T value, ref T valueRef, PropertyInfo propertySelector, IEqualityComparer<T> comparer)
        {
            var changed = _changeTrackingEnabled && comparer.Equals(valueRef, value) == false;

            valueRef = value;

            if (changed)
                OnPropertyChanged(propertySelector);
        }

        /// <summary>
        /// Detects changes and manages the dirty flag.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The new value.</param>
        /// <param name="orig">The original value.</param>
        /// <param name="propertySelector">The property selector.</param>
        /// <param name="comparer">A comparer to compare property values.</param>
        /// <param name="changed">A value indicating whether we know values have changed and no comparison is required.</param>
        internal void DetectChanges<T>(T value, T orig, PropertyInfo propertySelector, IEqualityComparer<T> comparer, bool changed)
        {
            if (_changeTrackingEnabled == false)
                return;

            if (!changed)
                changed = comparer.Equals(orig, value) == false;

            if (changed)
                OnPropertyChanged(propertySelector);
        }
    }
}
