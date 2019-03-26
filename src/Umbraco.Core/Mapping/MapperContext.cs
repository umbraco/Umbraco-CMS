using System.Collections.Generic;

namespace Umbraco.Core.Mapping
{
    /// <summary>
    /// Represents a mapper context.
    /// </summary>
    public class MapperContext
    {
        private IDictionary<string, object> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapperContext"/> class.
        /// </summary>
        public MapperContext(Mapper mapper)
        {
            Mapper = mapper;
        }

        /// <summary>
        /// Gets the mapper.
        /// </summary>
        public Mapper Mapper { get;}

        /// <summary>
        /// Gets a value indicating whether the context has items.
        /// </summary>
        public bool HasItems => _items != null;

        /// <summary>
        /// Gets the context items.
        /// </summary>
        public IDictionary<string, object> Items => _items ?? (_items = new Dictionary<string, object>());
    }
}
