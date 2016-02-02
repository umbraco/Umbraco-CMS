using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// A List that can be deep cloned with deep cloned elements and can reset the collection's items dirty flags
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DeepCloneableList<T> : List<T>, IDeepCloneable, IRememberBeingDirty
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
                        var dc = item as IDeepCloneable;
                        if (dc != null)
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
                        var dc = item as IDeepCloneable;
                        if (dc != null)
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

        public bool IsDirty()
        {
            return this.OfType<IRememberBeingDirty>().Any(x => x.IsDirty());
        }

        public bool WasDirty()
        {
            return this.OfType<IRememberBeingDirty>().Any(x => x.WasDirty());
        }

        /// <summary>
        /// Always returns false, the list has no properties we need to report
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public bool IsPropertyDirty(string propName)
        {
            return false;
        }

        /// <summary>
        /// Always returns false, the list has no properties we need to report
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool WasPropertyDirty(string propertyName)
        {
            return false;
        }

        public void ResetDirtyProperties()
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ResetDirtyProperties();
            }
        }

        public void ForgetPreviouslyDirtyProperties()
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ForgetPreviouslyDirtyProperties();
            }
        }

        public void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            foreach (var dc in this.OfType<IRememberBeingDirty>())
            {
                dc.ResetDirtyProperties(rememberPreviouslyChangedProperties);
            }
        }
    }
}
