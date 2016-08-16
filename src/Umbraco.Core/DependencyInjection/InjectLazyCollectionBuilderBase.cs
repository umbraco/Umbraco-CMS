using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;

namespace Umbraco.Core.DependencyInjection
{
    public abstract class InjectLazyCollectionBuilderBase<TCollection, TItem> : InjectCollectionBuilderBase<TCollection, TItem>
        where TCollection : IInjectCollection<TItem>
    {
        private readonly List<Func<IEnumerable<Type>>> _producers = new List<Func<IEnumerable<Type>>>();
        private readonly List<Type> _excluded = new List<Type>();

        protected InjectLazyCollectionBuilderBase(IServiceContainer container)
            : base(container)
        { }

        protected override IEnumerable<Type> PrepareTypes(IEnumerable<Type> types)
        {
            return types.Union(_producers.SelectMany(x => x())).Distinct().Except(_excluded);
        }

        public void AddProducer(Func<IEnumerable<Type>> producer)
        {
            Configure(() =>
            {
                _producers.Add(producer);
            });
        }

        public void Exclude<T>()
        {
            Configure(() =>
            {
                var type = typeof(T);
                if (!_excluded.Contains(type))
                    _excluded.Add(type);
            });
        }
    }
}