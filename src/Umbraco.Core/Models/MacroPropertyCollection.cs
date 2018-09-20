﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Collections;

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

        /// <summary>
        /// Used to update an existing macro property
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sortOrder"></param>
        /// <param name="editorAlias"></param>
        /// <param name="currentAlias">
        /// The existing property alias
        /// </param>
        /// <param name="newAlias"></param>
        public void UpdateProperty(string currentAlias, string name = null, int? sortOrder = null, string editorAlias = null, string newAlias = null)
        {
            var prop = this[currentAlias];
            if (prop == null)
            {
                throw new InvalidOperationException("No property exists with alias " + currentAlias);
            }

            if (name.IsNullOrWhiteSpace() == false)
            {
                prop.Name = name;
            }
            if (sortOrder.HasValue)
            {
                prop.SortOrder = sortOrder.Value;
            }
            if (name.IsNullOrWhiteSpace() == false)
            {
                prop.EditorAlias = editorAlias;
            }

            if (newAlias.IsNullOrWhiteSpace() == false && currentAlias != newAlias)
            {
                prop.Alias = newAlias;
                ChangeKey(currentAlias, newAlias);
            }
        }
    }

}
