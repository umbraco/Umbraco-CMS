using System;
using System.Collections.Generic;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a base class for collections of types.
    /// </summary>
    public abstract class TypeCollectionBuilderBase<TCollection, TConstraint> : ICollectionBuilder<TCollection, Type>
        where TCollection : class, IBuilderCollection<Type>
    {
        private readonly HashSet<Type> _types = new HashSet<Type>();

        private Type Validate(Type type, string action)
        {
            if (!typeof(TConstraint).IsAssignableFrom(type))
                throw new InvalidOperationException($"Cannot {action} type {type.FullName} as it does not inherit from/implement {typeof(TConstraint).FullName}.");
            return type;
        }

        public void Add(Type type) => _types.Add(Validate(type, "add"));

        public void Add<T>() => Add(typeof(T));

        public void Add(IEnumerable<Type> types)
        {
            foreach (var type in types) Add(type);
        }

        public void Remove(Type type) => _types.Remove(Validate(type, "remove"));

        public void Remove<T>() => Remove(typeof(T));

        public TCollection CreateCollection(IFactory factory)
        {
            return factory.CreateInstance<TCollection>(_types);
        }

        public void RegisterWith(IRegister register)
        {
            register.Register(CreateCollection, Lifetime.Singleton);
        }
    }
}
