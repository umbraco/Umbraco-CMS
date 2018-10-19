using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Umbraco.Core.Collections;

namespace Umbraco.Core.Models
{
    

    /// <summary>
    /// The culture names of a content's variants
    /// </summary>
    public class CultureNameCollection : ObservableDictionary<string, CultureName>, IDeepCloneable
    {
        /// <summary>
        /// Creates a new collection from another collection
        /// </summary>
        /// <param name="names"></param>
        public CultureNameCollection(IEnumerable<CultureName> names)
            : base(x => x.Culture, StringComparer.InvariantCultureIgnoreCase)
        {
            foreach (var n in names)
                Add(n);
        }

        /// <summary>
        /// Creates a new collection
        /// </summary>
        public CultureNameCollection()
             : base(x => x.Culture, StringComparer.InvariantCultureIgnoreCase)
        {
        }
        
        /// <summary>
        /// Add or update the <see cref="CultureName"/>
        /// </summary>
        /// <param name="name"></param>
        public void AddOrUpdate(string culture, string name, DateTime date)
        {
            culture = culture.ToLowerInvariant();
            if (TryGetValue(culture, out var found))
            {
                found.Name = name;
                found.Date = date;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, found, found));
            }
            else
                Add(new CultureName(culture)
                {
                    Name = name,
                    Date = date
                });    
        }

        public object DeepClone()
        {
            var clone = new CultureNameCollection();
            foreach (var name in this)
            {
                name.DisableChangeTracking();
                var copy = (CultureName)name.DeepClone();
                copy.ResetDirtyProperties(false);
                clone.Add(copy);
                name.EnableChangeTracking();
            }
            return clone;
        }
        
        
    }
}
