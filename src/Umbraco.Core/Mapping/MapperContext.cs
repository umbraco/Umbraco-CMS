using System;
using System.Collections.Generic;

namespace Umbraco.Core.Mapping
{
    /// <summary>
    /// Represents a mapper context.
    /// </summary>
    public class MapperContext
    {
        private readonly UmbracoMapper _mapper;
        private IDictionary<string, object> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperContext"/> class.
        /// </summary>
        public MapperContext(UmbracoMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Gets a value indicating whether the context has items.
        /// </summary>
        public bool HasItems => _items != null;

        /// <summary>
        /// Gets the context items.
        /// </summary>
        public IDictionary<string, object> Items => _items ?? (_items = new Dictionary<string, object>());

        #region Map

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TTarget>(object source)
            => _mapper.Map<TTarget>(source, this);

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="f">A mapper context preparation method.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TTarget>(object source, Action<MapperContext> f)
        {
            f(this);
            return _mapper.Map<TTarget>(source, this);
        }

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source)
            => _mapper.Map<TSource, TTarget>(source, this);

        /// <summary>
        /// Maps a source object to a new target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="f">A mapper context preparation method.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, Action<MapperContext> f)
        {
            f(this);
            return _mapper.Map<TSource, TTarget>(source, this);
        }

        /// <summary>
        /// Maps a source object to an existing target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target)
            => _mapper.Map(source, target, this);

        /// <summary>
        /// Maps a source object to an existing target object.
        /// </summary>
        /// <typeparam name="TSource">The source type.</typeparam>
        /// <typeparam name="TTarget">The target type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="target">The target object.</param>
        /// <param name="f">A mapper context preparation method.</param>
        /// <returns>The target object.</returns>
        public TTarget Map<TSource, TTarget>(TSource source, TTarget target, Action<MapperContext> f)
        {
            f(this);
            return _mapper.Map(source, target, this);
        }

        #endregion
    }
}
