using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Umbraco.Core.Logging;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
    /// The base class for all lazy many-objects resolvers.
    /// </summary>
    /// <typeparam name="TResolver">The type of the concrete resolver class.</typeparam>
    /// <typeparam name="TResolved">The type of the resolved objects.</typeparam>
    /// <remarks>
    /// <para>This is a special case resolver for when types get lazily resolved in order to resolve the actual types. This is useful
    /// for when there is some processing overhead (i.e. Type finding in assemblies) to return the Types used to instantiate the instances. 
    /// In some these cases we don't want to have to type-find during application startup, only when we need to resolve the instances.</para>
    /// <para>Important notes about this resolver: it does not support Insert or Remove and therefore does not support any ordering unless 
    /// the types are marked with the WeightedPluginAttribute.</para>
    /// </remarks>
    public abstract class LazyManyObjectsResolverBase<TResolver, TResolved> : ManyObjectsResolverBase<TResolver, TResolved>
        where TResolved : class
        where TResolver : ResolverBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects,
        /// and an optional lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="scope">The lifetime scope of instantiated objects, default is per Application.</param>
        /// <remarks>If <paramref name="scope"/> is per HttpRequest then there must be a current HttpContext.</remarks>
        /// <exception cref="InvalidOperationException"><paramref name="scope"/> is per HttpRequest but the current HttpContext is null.</exception>
        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(serviceProvider, logger, scope)
        {
            Initialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(scope)
        {
            Initialize();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(HttpContextBase httpContext)
            : base(httpContext)
        {
            Initialize();
        }

        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Lazy<Type>> lazyTypeList, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(serviceProvider, logger, scope)
        {
            AddTypes(lazyTypeList);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(IEnumerable<Lazy<Type>> lazyTypeList, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, lazyTypeList, scope)
        {
        }

        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> typeListProducerList, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(serviceProvider, logger, scope)
        {
            _typeListProducerList.Add(typeListProducerList);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(Func<IEnumerable<Type>> typeListProducerList, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, typeListProducerList, scope)
        {
        }

        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext, IEnumerable<Lazy<Type>> lazyTypeList)
            : this(serviceProvider, logger, httpContext)
        {
            AddTypes(lazyTypeList);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(HttpContextBase httpContext, IEnumerable<Lazy<Type>> lazyTypeList)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, httpContext, lazyTypeList)
        {
        }

        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext, Func<IEnumerable<Type>> typeListProducerList)
            : this(serviceProvider, logger, httpContext)
        {
            _typeListProducerList.Add(typeListProducerList);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use ctor specifying IServiceProvider instead")]
        protected LazyManyObjectsResolverBase(HttpContextBase httpContext, Func<IEnumerable<Type>> typeListProducerList)
            : this(new ActivatorServiceProvider(), LoggerResolver.Current.Logger, httpContext, typeListProducerList)
        {
        }

        #endregion

        private readonly List<Lazy<Type>> _lazyTypeList = new List<Lazy<Type>>();
        private readonly List<Func<IEnumerable<Type>>> _typeListProducerList = new List<Func<IEnumerable<Type>>>();
        private readonly List<Type> _excludedTypesList = new List<Type>();
        private Lazy<List<Type>> _resolvedTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an empty list of objects,
        /// with creation of objects based on an HttpRequest lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="httpContext">The HttpContextBase corresponding to the HttpRequest.</param>
        /// <exception cref="ArgumentNullException"><paramref name="httpContext"/> is <c>null</c>.</exception>
        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, HttpContextBase httpContext)
            : base(serviceProvider, logger, httpContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManyObjectsResolverBase{TResolver, TResolved}"/> class with an initial list of object types,
        /// and an optional lifetime scope.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="value">The list of object types.</param>
        /// <param name="scope">The lifetime scope of instantiated objects, default is per Application.</param>
        /// <remarks>If <paramref name="scope"/> is per HttpRequest then there must be a current HttpContext.</remarks>
        /// <exception cref="InvalidOperationException"><paramref name="scope"/> is per HttpRequest but the current HttpContext is null.</exception>
        protected LazyManyObjectsResolverBase(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> value, ObjectLifetimeScope scope = ObjectLifetimeScope.Application)
            : base(serviceProvider, logger, value, scope)
        {
        }

        private void Initialize()
        {
            _resolvedTypes = new Lazy<List<Type>>(() =>
            {
                var resolvedTypes = new List<Type>();

                // get the types by evaluating the lazy & producers
                var types = new List<Type>();
                types.AddRange(_lazyTypeList.Select(x => x.Value));
                types.AddRange(_typeListProducerList.SelectMany(x => x()));

                // we need to validate each resolved type now since we could
                // not do it before evaluating the lazy & producers
                foreach (var type in types.Where(x => _excludedTypesList.Contains(x) == false))
                {
                    AddValidAndNoDuplicate(resolvedTypes, type);
                }

                return resolvedTypes;
            });
        }

        /// <summary>
        /// Gets a value indicating whether the resolver has resolved types to create instances from.
        /// </summary>
        /// <remarks>To be used in unit tests.</remarks>
        public bool HasResolvedTypes
        {
            get { return _resolvedTypes.IsValueCreated; }
        }

        /// <summary>
        /// Gets the list of types to create instances from.
        /// </summary>
        /// <remarks>When called, will get the types from the lazy list.</remarks>
        protected override IEnumerable<Type> InstanceTypes
        {
            get { return _resolvedTypes.Value; }
        }

        /// <summary>
        /// Ensures that type is valid and not a duplicate
        /// then appends the type to the end of the list
        /// </summary>
        /// <param name="list"></param>
        /// <param name="type"></param>
        private void AddValidAndNoDuplicate(List<Type> list, Type type)
        {
            EnsureCorrectType(type);
            if (list.Contains(type))
            {
                throw new InvalidOperationException(string.Format(
                    "Type {0} is already in the collection of types.", type.FullName));
            }
            list.Add(type);
        }

        #region Types collection manipulation

        /// <summary>
        /// Removes types from the list of types, once it has been lazily evaluated, and before actual objects are instanciated.
        /// </summary>
        /// <param name="value">The type to remove.</param>
        public override void RemoveType(Type value)
        {
            EnsureSupportsRemove();

            _excludedTypesList.Add(value);
        }

        /// <summary>
        /// Lazily adds types from lazy types.
        /// </summary>
        /// <param name="types">The lazy types, to add.</param>
        protected void AddTypes(IEnumerable<Lazy<Type>> types)
        {
            EnsureSupportsAdd();

            using (Resolution.Configuration)
            using (GetWriteLock())
            {
                foreach (var t in types)
                {
                    _lazyTypeList.Add(t);
                }
            }
        }

        /// <summary>
        /// Lazily adds types from a function producing types.
        /// </summary>
        /// <param name="typeListProducer">The functions producing types, to add.</param>
        public void AddTypeListDelegate(Func<IEnumerable<Type>> typeListProducer)
        {
            EnsureSupportsAdd();

            using (Resolution.Configuration)
            using (GetWriteLock())
            {
                _typeListProducerList.Add(typeListProducer);
            }
        }

        /// <summary>
        /// Lazily adds a type from a lazy type.
        /// </summary>
        /// <param name="value">The lazy type, to add.</param>
        public void AddType(Lazy<Type> value)
        {
            EnsureSupportsAdd();

            using (Resolution.Configuration)
            using (GetWriteLock())
            {
                _lazyTypeList.Add(value);
            }
        }

        /// <summary>
        /// Lazily adds a type from an actual type.
        /// </summary>
        /// <param name="value">The actual type, to add.</param>
        /// <remarks>The type is converted to a lazy type.</remarks>
        public override void AddType(Type value)
        {
            AddType(new Lazy<Type>(() => value));
        }

        /// <summary>
        /// Clears all lazy types
        /// </summary>
        public override void Clear()
        {
            EnsureSupportsClear();

            using (Resolution.Configuration)
            using (GetWriteLock())
            {
                _lazyTypeList.Clear();
            }
        }

        #endregion

        #region Types collection manipulation support

        /// <summary>
        /// Gets a <c>false</c> value indicating that the resolver does NOT support inserting types.
        /// </summary>
        protected override bool SupportsInsert
        {
            get { return false; }
        }

        #endregion
    }
}