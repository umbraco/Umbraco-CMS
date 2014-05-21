using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A property extension class that allows us to enable tags for any given property
    /// </summary>
    internal class PropertyTags
    {
        public PropertyTags()
        {
            Enable = false;
            Behavior = PropertyTagBehavior.Merge;
        }

        /// <summary>
        /// The behavior of how to save the tags assigned - 
        ///    Merge (keep existing and append new), 
        ///    Remove (remove any of the tags in the Tags property that are currently assigned, 
        ///    Replace (replace the currently assigned tags with the ones specified)
        /// </summary>
        public PropertyTagBehavior Behavior { get; set; }

        /// <summary>
        /// Flags the property to have tagging enabled
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// The actual tags to associate - tag/group
        /// </summary>
        public IEnumerable<Tuple<string, string>> Tags { get; set; }

    }
}