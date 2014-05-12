using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A macro's property collection
    /// </summary>
    public class MacroPropertyCollection : ObservableDictionary<string, IMacroProperty>, IDeepCloneable
    {
        public MacroPropertyCollection() 
            : base(property => property.Alias)
        {
        }

        public object DeepClone()
        {
            var clone = new MacroPropertyCollection();
            foreach (var item in this)
            {
                clone.Add((IMacroProperty)item.DeepClone());
            }
            return clone;
        }
    }

}