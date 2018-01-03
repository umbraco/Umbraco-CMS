using System;
using System.Collections.Generic;

namespace Umbraco.Web.Features
{
    /// <summary>
    /// Maintains a list of strongly typed types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeList<T>
    {
        private readonly List<Type> _disabled = new List<Type>();

        public void Add<TType>()
            where TType : T
        {
            _disabled.Add(typeof(TType));
        }

        public bool Contains(Type type)
        {
            return _disabled.Contains(type);
        }
    }
}