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
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1"/> class that is empty and has the default initial capacity.
        /// </summary>
        public DeepCloneableList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param><exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public DeepCloneableList(IEnumerable<T> collection) : base(collection)
        {
        }

        /// <summary>
        /// Creates a new list and adds each element as a deep cloned element if it is of type IDeepCloneable
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            var newList = new DeepCloneableList<T>();
            foreach (var item in this)
            {
                var dc = item as IDeepCloneable;
                if (dc != null)
                {
                    newList.Add((T) dc.DeepClone());
                }
                else
                {
                    newList.Add(item);
                }
            }
            return newList;
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
