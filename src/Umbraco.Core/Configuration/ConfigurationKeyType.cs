using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Indicates the type of configuration section keys.
    /// </summary>
    internal enum ConfigurationKeyType
    {
        /// <summary>
        /// An Umbraco section ie with path "/umbraco/sectionKey".
        /// </summary>
        Umbraco,

        /// <summary>
        /// An Umbraco plugins section ie with path "/umbraco.plugins/sectionKey".
        /// </summary>
        Plugins,

        /// <summary>
        /// A raw section ie with path "/sectionKey".
        /// </summary>
        Raw
    }
}
