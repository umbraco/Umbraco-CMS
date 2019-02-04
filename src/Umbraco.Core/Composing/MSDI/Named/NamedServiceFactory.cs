using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Umbraco.Core.Composing.MSDI.Named
{
    /*
    MIT License

    Copyright (c) 2019 James Jackson-South

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    */

    /// <summary>
    /// A factory for resolving named services.
    /// </summary>
    internal interface INamedServiceFactory
    {
        /// <summary>
        /// Gets the service type.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Resolves the service type matching the given name. 
        /// </summary>
        /// <param name="name">The name the service type is registered as.</param>
        /// <param name="provider">The service provider for retrieving service objects.</param>
        /// <returns>The <see cref="object"/></returns>
        object Resolve(string name, IServiceProvider provider);
    }

    /// <summary>
    /// A factory for resolving named services.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve.</typeparam>
    internal class NamedServiceFactory<TService> : INamedServiceFactory
    {
        private readonly ConcurrentDictionary<CompositeStringTypeKey, Type> namedServices = new ConcurrentDictionary<CompositeStringTypeKey, Type>();

        /// <inheritdoc/>
        public Type ServiceType { get; } = typeof(TService);

        /// <summary>
        /// Registers the given implementation type with the name.
        /// </summary>
        /// <typeparam name="TImplementation">The implemention type.</typeparam>
        /// <param name="name">The name to register the type as.</param>
        public void Register<TImplementation>(string name)
            where TImplementation : class
            => this.namedServices.TryAdd(new CompositeStringTypeKey(name, typeof(TService)), typeof(TImplementation));

        /// <summary>
        /// Resolves the service type matching the given name. 
        /// </summary>
        /// <param name="provider">The service provider for retrieving service objects.</param>
        /// <param name="name">The name the service type is registered as.</param>
        /// <returns>The <see cref="TService"/></returns>
        public TService Resolve(IServiceProvider provider, string name) => (TService)this.Resolve(name, provider);

        /// <inheritdoc/>
        public object Resolve(string name, IServiceProvider provider)
        {
            if (!this.namedServices.TryGetValue(new CompositeStringTypeKey(name, typeof(TService)), out Type implementation))
            {
                throw new InvalidOperationException($"No service for type {typeof(TService)} named '{name}' has been registered.");
            }

            return provider.GetService(implementation);
        }

        /// <summary>
        /// Used for super fast dictionary key lookups.
        /// </summary>
        private readonly struct CompositeStringTypeKey : IEquatable<CompositeStringTypeKey>
        {
            public CompositeStringTypeKey(string name, Type type)
            {
                this.Name = name;
                this.Type = type;
            }

            public string Name { get; }
            public Type Type { get; }

            public override bool Equals(object obj) => obj is CompositeStringTypeKey nameTypeKey && this.Equals(nameTypeKey);

            public bool Equals(CompositeStringTypeKey other) => this.Name == other.Name && EqualityComparer<Type>.Default.Equals(this.Type, other.Type);

            public override int GetHashCode() => String.Join("", this.Name, this.Type).GetHashCode();
        }
    }
}
