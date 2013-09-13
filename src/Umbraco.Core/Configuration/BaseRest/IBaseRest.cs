using System.Collections.Generic;

namespace Umbraco.Core.Configuration.BaseRest
{
    public interface IBaseRest
    {
        IExtensionsCollection Items { get; }

        /// <summary>
        /// Gets a value indicating whether base rest extensions are enabled.
        /// </summary>
        bool Enabled { get; }
    }
}