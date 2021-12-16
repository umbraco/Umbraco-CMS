using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Collections
{
    /// <summary>
    /// A List that can be deep cloned with deep cloned elements and can reset the collection's items dirty flags
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DeepCloneableList<T> : List<T>, IDeepCloneable, IRememberBeingDirty
    {
        private readonly ListCloneBehavior _listCloneBehavior;

        public DeepCloneableList(ListCloneBehavior listCloneBehavior)
        {
            _listCloneBehavior = listCloneBehavior;
        }

        public DeepCloneableList(IEnumerable<T> collection, ListCloneBehavior listCloneBehavior) : base(collection)
        {
            _listCloneBehavior = listCloneBehavior;
        }

        /// <summary>
        /// Default behavior is CloneOnce
        /// </summary>
        /// <param name="collection"></param>
        public DeepCloneableList(IEnumerable<T> collection)
            : this(collection, ListCloneBehavior.CloneOnce)
        {
        }

        /// <summary>
        /// Creates a new list and adds each element as a deep cloned element if it is of type IDeepCloneable
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            switch (_listCloneBehavior)
            {
                case ListCloneBehavior.CloneOnce:
                    //we are cloning once, so create a new list in none mode
                    // and deep clone all items into it
                    var newList = new DeepCloneableList<T>(ListCloneBehavior.None);
                    foreach (var item in this)
                    {
                        if (item is IDeepCloneable dc)
                        {
                            newList.Add((T)dc.DeepClone());
                        }
                        else
                        {
                            newList.Add(item);
                        }
                    }
                    return newList;
                case ListCloneBehavior.None:
                    //we are in none mode, so just return a new list with the same items
                    return new DeepCloneableList<T>(this, ListCloneBehavior.None);
                case ListCloneBehavior.Always:
                    //always clone to new list
                    var newList2 = new DeepCloneableList<T>(ListCloneBehavior.Always);
                    foreach (var item in this)
                    {
                        if (item is IDeepCloneable dc)
                        {
                            newList2.Add((T)dc.DeepClone());
                        }
                        else
                        {
                            newList2.Add(item);
                        }
                    }
                    return newList2;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region IRememberBeingDirty
        public bool IsDirty()
        {
            return this.OfType<IRememberBeingDirty>().Any(x => x.IsDirty());
        }

        public bool WasDirty()
        {
            return this.OfType<IRememberBeingDirty>().Any(x => x.WasDirty());
        }

        /// <inheritdoc />
        /// <remarks>Always return false, the list has no properties that can be dirty.</remarks>
        public bool IsPropertyDirty(string propName)
        {
            return false;
        }

        /// <inheritdoc />
        /// <remarks>Always return false, the list has no properties that can be dirty.</remarks>
        public bool WasPropertyDirty(string propertyName)
        {
            return false;
        }

        /// <inheritdoc />
        /// <remarks>Always return an empty enumerable, the list has no properties that can be dirty.</remarks>
        public IEnumerable<string> GetDirtyProperties()
        {
            return Enumerable.Empty<string>();
        }

        public void ResetDirtyProperties()
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ResetDirtyProperties();
            }
        }

        public void DisableChangeTracking()
        {
            // noop
        }

        public void EnableChangeTracking()
        {
            // noop
        }

        public void ResetWereDirtyProperties()
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ResetWereDirtyProperties();
            }
        }

        public void ResetDirtyProperties(bool rememberDirty)
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ResetDirtyProperties(rememberDirty);
            }
        }

        /// <remarks>Always return an empty enumerable, the list has no properties that can be dirty.</remarks>
        public IEnumerable<string> GetWereDirtyProperties()
        {
            return Enumerable.Empty<string>();
        }

        public event PropertyChangedEventHandler? PropertyChanged; // noop
        #endregion
    }
}
