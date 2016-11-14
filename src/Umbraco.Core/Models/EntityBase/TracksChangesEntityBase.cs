using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// A base class for use to implement IRememberBeingDirty/ICanBeDirty
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class TracksChangesEntityBase : IRememberBeingDirty
    {
        //TODO: This needs to go on to ICanBeDirty http://issues.umbraco.org/issue/U4-5662
        public virtual IEnumerable<string> GetDirtyProperties()
        {
            return _propertyChangedInfo == null
                ? Enumerable.Empty<string>()
                : _propertyChangedInfo.Where(x => x.Value).Select(x => x.Key);
        }

        private bool _changeTrackingEnabled = true;

        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        private IDictionary<string, bool> _propertyChangedInfo;

        /// <summary>
        /// Tracks the properties that we're changed before the last commit (or last call to ResetDirtyProperties)
        /// </summary>
        private IDictionary<string, bool> _lastPropertyChangedInfo;

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call on a property setter.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            //return if we're not tracking changes
            if (_changeTrackingEnabled == false) return;

            if (_propertyChangedInfo == null)
                _propertyChangedInfo = new Dictionary<string, bool>();

            _propertyChangedInfo[propertyInfo.Name] = true;

            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        /// <summary>
        /// Indicates whether a specific property on the current entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public virtual bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo != null && _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public virtual bool IsDirty()
        {
            return _propertyChangedInfo != null && _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Indicates that the entity had been changed and the changes were committed
        /// </summary>
        /// <returns></returns>
        public virtual bool WasDirty()
        {
            return _lastPropertyChangedInfo != null && _lastPropertyChangedInfo.Any();
        }

        /// <summary>
        /// Indicates whether a specific property on the current entity was changed and the changes were committed
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property was changed, otherwise False. Returns false if the entity had not been previously changed.</returns>
        public virtual bool WasPropertyDirty(string propertyName)
        {
            return _lastPropertyChangedInfo != null && _lastPropertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Resets the remembered dirty properties from before the last commit
        /// </summary>
        public void ForgetPreviouslyDirtyProperties()
        {
            //NOTE: We cannot .Clear() because when we memberwise clone this will be the SAME
            // instance as the one on the clone, so we need to create a new instance.
            _lastPropertyChangedInfo = null;
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        /// <remarks>
        /// Please note that resetting the dirty properties could potentially
        /// obstruct the saving of a new or updated entity.
        /// </remarks>
        public virtual void ResetDirtyProperties()
        {
            ResetDirtyProperties(true);
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        /// <param name="rememberPreviouslyChangedProperties">
        /// true if we are to remember the last changes made after resetting
        /// </param>
        /// <remarks>
        /// Please note that resetting the dirty properties could potentially
        /// obstruct the saving of a new or updated entity.
        /// </remarks>
        public virtual void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            if (rememberPreviouslyChangedProperties)
            {
                //copy the changed properties to the last changed properties
                if (_propertyChangedInfo != null)
                {
                    _lastPropertyChangedInfo = _propertyChangedInfo.ToDictionary(v => v.Key, v => v.Value);
                }
            }

            //NOTE: We cannot .Clear() because when we memberwise clone this will be the SAME
            // instance as the one on the clone, so we need to create a new instance.
            _propertyChangedInfo = null;
        }

        public void ResetChangeTrackingCollections()
        {
            _propertyChangedInfo = null;
            _lastPropertyChangedInfo = null;
        }

        public void DisableChangeTracking()
        {
            _changeTrackingEnabled = false;
        }

        public void EnableChangeTracking()
        {
            _changeTrackingEnabled = true;
        }

        /// <summary>
        /// Used by inheritors to set the value of properties, this will detect if the property value actually changed and if it did
        /// it will ensure that the property has a dirty flag set.
        /// </summary>
        /// <param name="newVal"></param>
        /// <param name="origVal"></param>
        /// <param name="propertySelector"></param>
        /// <returns>returns true if the value changed</returns>
        /// <remarks>
        /// This is required because we don't want a property to show up as "dirty" if the value is the same. For example, when we
        /// save a document type, nearly all properties are flagged as dirty just because we've 'reset' them, but they are all set
        /// to the same value, so it's really not dirty.
        /// </remarks>
        internal void SetPropertyValueAndDetectChanges<T>(T newVal, ref T origVal, PropertyInfo propertySelector)
        {
            if ((typeof(T) == typeof(string) == false) && TypeHelper.IsTypeAssignableFrom<IEnumerable>(typeof(T)))
            {
                throw new InvalidOperationException("This method does not support IEnumerable instances. For IEnumerable instances a manual custom equality check will be required");
            }

            SetPropertyValueAndDetectChanges(newVal, ref origVal, propertySelector, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Used by inheritors to set the value of properties, this will detect if the property value actually changed and if it did
        /// it will ensure that the property has a dirty flag set.
        /// </summary>
        /// <param name="newVal"></param>
        /// <param name="origVal"></param>
        /// <param name="propertySelector"></param>
        /// <param name="comparer">The equality comparer to use</param>
        /// <returns>returns true if the value changed</returns>
        /// <remarks>
        /// This is required because we don't want a property to show up as "dirty" if the value is the same. For example, when we
        /// save a document type, nearly all properties are flagged as dirty just because we've 'reset' them, but they are all set
        /// to the same value, so it's really not dirty.
        /// </remarks>
        internal void SetPropertyValueAndDetectChanges<T>(T newVal, ref T origVal, PropertyInfo propertySelector, IEqualityComparer<T> comparer)
        {
            //don't track changes, just set the value
            if (_changeTrackingEnabled == false)
            {
                //set the original value
                origVal = newVal;
            }
            else
            {
                //check changed
                var changed = comparer.Equals(origVal, newVal) == false;

                //set the original value
                origVal = newVal;

                //raise the event if it was changed
                if (changed)
                {
                    OnPropertyChanged(propertySelector);
                }
            }
        }
    }
}