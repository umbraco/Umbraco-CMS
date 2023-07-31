using Umbraco.Search.Extensions;

namespace Umbraco.Search.ValueSet;

public class UmbracoValueSet
{
       /// <summary>
        /// The id of the object to be indexed
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The index category
        /// </summary>
        /// <remarks>
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </remarks>
        public string? Category { get; }

        /// <summary>
        /// The item's node type (in umbraco terms this would be the doc type alias)
        /// </summary>
        public string? ItemType { get; }

        /// <summary>
        /// The values to be indexed
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<object>>? Values { get; }

        /// <summary>
        /// Constructor that only specifies an ID
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>normally used for deletions</remarks>
        public UmbracoValueSet(string id) => Id = id;

        public static UmbracoValueSet FromObject(string id, string category, string itemType, object values)
            => new UmbracoValueSet(id, category, itemType, ObjectExtensions.ConvertObjectToDictionary(values));

        public static UmbracoValueSet FromObject(string id, string category, object values)
            => new UmbracoValueSet(id, category, ObjectExtensions.ConvertObjectToDictionary(values));

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public UmbracoValueSet(string id, string category, IDictionary<string, object> values)
            : this(id, category, string.Empty, values)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="itemType"></param>
        /// <param name="values"></param>
        public UmbracoValueSet(string id, string category, string itemType, IDictionary<string, object> values)
            : this(id, category, itemType, values.ToDictionary(x => x.Key, x => Yield(x.Value)))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public UmbracoValueSet(string id, string category, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, string.Empty, values)
        {
        }

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemType">
        /// The item's node type (in umbraco terms this would be the doc type alias)</param>
        /// <param name="category">
        /// Used to categorize the item in the index (in umbraco terms this would be content vs media)
        /// </param>
        /// <param name="values"></param>
        public UmbracoValueSet(string id, string category, string itemType, IDictionary<string, IEnumerable<object>> values)
            : this(id, category, itemType, values.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Value.ToList()))
        {
        }

        private UmbracoValueSet(string id, string category, string itemType, IReadOnlyDictionary<string, IReadOnlyList<object>> values)
        {
            Id = id;
            Category = category;
            ItemType = itemType;
            Values = values.ToDictionary(x => x.Key, x => (IReadOnlyList<object>)x.Value.ToList());
        }

        /// <summary>
        /// Gets the values for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<object> GetValues(string key)
        {
            if (Values == null)
            {
                return new List<object>();
            }
            return !Values.TryGetValue(key, out var values) ? Enumerable.Empty<object>() : values;
        }

        /// <summary>
        /// Gets a single value for the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// If there are multiple values, this will return the first
        /// </returns>
        public object? GetValue(string key)
        {
            if (Values == null)
            {
                return null;
            }
            return  !Values.TryGetValue(key, out var values) ? null : values?.Count > 0 ? values[0] : null;
        }

        /// <summary>
        /// Helper method to return IEnumerable from a single
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private static IEnumerable<object> Yield(object i)
        {
            yield return i;
        }

        public UmbracoValueSet Clone() => new UmbracoValueSet(Id, Category!, ItemType!, Values!);
}
