using System.Collections.ObjectModel;

namespace UmbracoExamine
{
    internal class StaticFieldCollection : KeyedCollection<string, StaticField>
    {
        protected override string GetKeyForItem(StaticField item)
        {
            return item.Name;
        }

        /// <summary>
        /// Implements TryGetValue using the underlying dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out StaticField field)
        {
            if (Dictionary == null)
            {
                field = null;
                return false;
            }
            return Dictionary.TryGetValue(key, out field);
        }
    }
}