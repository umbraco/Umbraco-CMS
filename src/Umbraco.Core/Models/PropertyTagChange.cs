using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// A set of tag changes.
    /// </summary>
    internal class PropertyTagChange
    {
        public ChangeType Type { get; set; }

        public IEnumerable<Tuple<string, string>> Tags { get; set; }

        public enum ChangeType
        {
            Replace,
            Remove,
            Merge
        }
    }
}
