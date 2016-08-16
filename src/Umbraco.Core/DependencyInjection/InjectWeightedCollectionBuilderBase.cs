using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    public abstract class InjectWeightedCollectionBuilderBase<TCollection, TItem> : InjectCollectionBuilderBase<TCollection, TItem>
        where TCollection : IInjectCollection<TItem>
    {
        protected InjectWeightedCollectionBuilderBase(IServiceContainer container)
            : base(container)
        { }

        protected override IEnumerable<Type> PrepareTypes(IEnumerable<Type> types)
        {
            var list = types.ToList();
            list.Sort((t1, t2) => GetWeight(t1).CompareTo(GetWeight(t2)));
            return list;
        }

        protected virtual int DefaultWeight { get; set; } = 10;

        protected virtual int GetWeight(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(WeightAttribute), false).OfType<WeightAttribute>().SingleOrDefault();
            return attr?.Weight ?? DefaultWeight;
        }
    }
}