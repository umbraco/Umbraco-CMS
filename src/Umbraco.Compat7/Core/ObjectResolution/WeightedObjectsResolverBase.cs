using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.ObjectResolution
{
    public class WeightedObjectsResolverBase<TBuilder, TCollection, TItem>
        where TBuilder : WeightedCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        private readonly TBuilder _builder;

        protected WeightedObjectsResolverBase(TBuilder builder)
        {
            _builder = builder;
        }

        public static bool HasCurrent => true;

        public void RemoveType(Type value)
        {
            _builder.Remove(value);
        }

        public void RemoveType<T>()
            where T : TItem
        {
            _builder.Remove<T>();
        }

        protected void AddTypes(IEnumerable<Type> types)
        {
            _builder.Add(types);
        }

        public void AddType(Type value)
        {
            _builder.Add(value);
        }

        public void AddType<T>()
            where T : TItem
        {
            _builder.Add<T>();
        }

        public void Clear()
        {
            _builder.Clear();
        }

        public void InsertType(int index, Type value)
        {
            _builder.Add(value);
        }

        public void InsertType(Type value)
        {
            _builder.Add(value);
        }

        public void InsertType<T>(int index)
            where T : TItem
        {
            _builder.Add<T>();
        }

        public void InsertType<T>()
            where T : TItem
        {
            _builder.Add<T>();
        }

        public void InsertTypeBefore(Type existingType, Type value)
        {
            _builder.Add(value);
        }

        public void InsertTypeBefore<TExisting, T>()
            where TExisting : TItem
            where T : TItem
        {
            _builder.Add<T>();
        }

        public bool ContainsType(Type value)
        {
            return _builder.Has(value);
        }

        public IEnumerable<Type> GetTypes()
        {
            return _builder.GetTypes();
        }

        public bool ContainsType<T>()
            where T : TItem
        {
            return _builder.Has<T>();
        }
    }
}
