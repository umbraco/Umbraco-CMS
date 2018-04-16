using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Macros
{
    // that one is special since it's not initialized with XsltExtension types, but with Xslt extension object types,
    // which are then wrapped in an XsltExtension object when the collection is created. so, cannot really inherit
    // from (Lazy)CollectionBuilderBase and have to re-implement it. but almost everything is copied from CollectionBuilderBase.

    internal class XsltExtensionCollectionBuilder : ICollectionBuilder<XsltExtensionCollection, XsltExtension>
    {
        private readonly IServiceContainer _container;
        private readonly List<Func<IEnumerable<Type>>> _producers = new List<Func<IEnumerable<Type>>>();
        private readonly object _locker = new object();
        private ServiceRegistration[] _registrations;

        public XsltExtensionCollectionBuilder(IServiceContainer container)
        {
            _container = container;

            // register the collection
            container.Register(_ => CreateCollection(), new PerContainerLifetime());
        }

        public static XsltExtensionCollectionBuilder Register(IServiceContainer container)
        {
            // register the builder - per container
            var builderLifetime = new PerContainerLifetime();
            container.Register<XsltExtensionCollectionBuilder>(builderLifetime);
            return container.GetInstance<XsltExtensionCollectionBuilder>();
        }

        public XsltExtensionCollectionBuilder AddExtensionObjectProducer(Func<IEnumerable<Type>> producer)
        {
            lock (_locker)
            {
                if (_registrations != null)
                    throw new InvalidOperationException("Cannot configure a collection builder after its types have been resolved.");
                _producers.Add(producer);
            }
            return this;
        }

        private void RegisterTypes()
        {
            lock (_locker)
            {
                if (_registrations != null) return;

                var prefix = GetType().FullName + "_";
                var i = 0;
                foreach (var type in _producers.SelectMany(x => x()).Distinct())
                {
                    var name = $"{prefix}{i++:00000}";
                    _container.Register(type, type, name);
                }

                _registrations = _container.AvailableServices
                    .Where(x => x.ServiceName.StartsWith(prefix))
                    .OrderBy(x => x.ServiceName)
                    .ToArray();
            }
        }

        public XsltExtensionCollection CreateCollection()
        {
            RegisterTypes(); // will do it only once

            var exts = _registrations.SelectMany(r => r.ServiceType.GetCustomAttributes<XsltExtensionAttribute>(true)
                .Select(a => new XsltExtension(a.Namespace.IfNullOrWhiteSpace(r.ServiceType.FullName), _container.GetInstance(r.ServiceType, r.ServiceName))));

            return new XsltExtensionCollection(exts);
        }
    }
}
