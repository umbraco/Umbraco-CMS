using Examine;
using System.Collections.Generic;

namespace Umbraco.Examine
{
    public static class ValueSetExtensions
    {
        public static ValueSet Clone(this ValueSet valueSet)
        {
            var values = new Dictionary<string, IEnumerable<object>>();
            foreach (var keyVal in valueSet.Values)
                values[keyVal.Key] = keyVal.Value;
            return new ValueSet(valueSet.Id, valueSet.Category, valueSet.ItemType, values);
        }
    }

}
