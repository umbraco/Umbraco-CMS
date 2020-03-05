using System;
using System.Collections.Generic;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// Represents a list of types.
    /// </summary>
    /// <remarks>Types in the list are, or derive from, or implement, the base type.</remarks>
    /// <typeparam name="TBase">The base type.</typeparam>
    public class TypeList<TBase>
    {
        private readonly List<Type> _list = new List<Type>();

        /// <summary>
        /// Adds a type to the list.
        /// </summary>
        /// <typeparam name="T">The type to add.</typeparam>
        public void Add<T>()
            where T : TBase
        {
            _list.Add(typeof(T));
        }

        /// <summary>
        /// Determines whether a type is in the list.
        /// </summary>
        public bool Contains(Type type)
        {
            return _list.Contains(type);
        }
    }
}
