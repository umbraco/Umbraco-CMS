using System;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates that an Assembly is a Models Builder assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly /*, AllowMultiple = false, Inherited = false*/)]
    public sealed class ModelsBuilderAssemblyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the assembly is a PureLive assembly.
        /// </summary>
        /// <remarks>A Models Builder assembly can be either PureLive or normal Dll.</remarks>
        public bool PureLive { get; set; }

        /// <summary>
        /// Gets or sets a hash value representing the state of the custom source code files
        /// and the Umbraco content types that were used to generate and compile the assembly.
        /// </summary>
        public string SourceHash { get; set; }
    }
}
