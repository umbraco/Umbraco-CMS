using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A macro's property collection
    /// </summary>
    public class MacroPropertyCollection : ObservableDictionary<string, IMacroProperty>
    {
        public MacroPropertyCollection() 
            : base(property => property.Alias)
        {
        }

    }

}