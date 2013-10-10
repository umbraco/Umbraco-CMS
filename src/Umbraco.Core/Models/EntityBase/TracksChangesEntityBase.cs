using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// A base class for use to implement IRememberBeingDirty/ICanBeDirty
    /// </summary>
    public abstract class TracksChangesEntityBase : IRememberBeingDirty
    {
        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        private readonly IDictionary<string, bool> _propertyChangedInfo = new Dictionary<string, bool>();

        /// <summary>
        /// Tracks the properties that we're changed before the last commit (or last call to ResetDirtyProperties)
        /// </summary>
        private IDictionary<string, bool> _lastPropertyChangedInfo = null;

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
            _propertyChangedInfo[propertyInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyInfo.Name));
            }
        }

        /// <summary>
        /// Indicates whether a specific property on the current entity is dirty.
        /// </summary>
        /// <param name="propertyName">Name of the property to check</param>
        /// <returns>True if Property is dirty, otherwise False</returns>
        public virtual bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Indicates whether the current entity is dirty.
        /// </summary>
        /// <returns>True if entity is dirty, otherwise False</returns>
        public virtual bool IsDirty()
        {
            return _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Indicates that the entity had been changed and the changes were committed
        /// </summary>
        /// <returns></returns>
        public bool WasDirty()
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
            return WasDirty() && _lastPropertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Resets the remembered dirty properties from before the last commit
        /// </summary>
        public void ForgetPreviouslyDirtyProperties()
        {
            _lastPropertyChangedInfo.Clear();
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
                _lastPropertyChangedInfo = _propertyChangedInfo.ToDictionary(v => v.Key, v => v.Value);
            }

            _propertyChangedInfo.Clear();
        }

        /// <summary>
        /// Used by inheritors to set the value of properties, this will detect if the property value actually changed and if it did
        /// it will ensure that the property has a dirty flag set.
        /// </summary>
        /// <param name="setValue"></param>
        /// <param name="value"></param>
        /// <param name="propertySelector"></param>
        /// <returns>returns true if the value changed</returns>
        /// <remarks>
        /// This is required because we don't want a property to show up as "dirty" if the value is the same. For example, when we 
        /// save a document type, nearly all properties are flagged as dirty just because we've 'reset' them, but they are all set 
        /// to the same value, so it's really not dirty.
        /// </remarks>
        internal bool SetPropertyValueAndDetectChanges<T>(Func<T, T> setValue, T value, PropertyInfo propertySelector)
        {
            var initVal = value;
            var newVal = setValue(value);
            if (!Equals(initVal, newVal))
            {
                OnPropertyChanged(propertySelector);
                return true;
            }
            return false;
        }
    }
}