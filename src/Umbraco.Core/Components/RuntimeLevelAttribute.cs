using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Marks a composer to indicate a minimum and/or maximum runtime level for which the composer would compose.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class /*, AllowMultiple = false, Inherited = true*/)]
    public class RuntimeLevelAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the minimum runtime level for which the composer would compose.
        /// </summary>
        public RuntimeLevel MinLevel { get; set; } = RuntimeLevel.Install;

        /// <summary>
        /// Gets or sets the maximum runtime level for which the composer would compose.
        /// </summary>
        public RuntimeLevel MaxLevel { get; set; } = RuntimeLevel.Run;
    }
}
