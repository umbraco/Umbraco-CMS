using System;
using System.Collections.Generic;
using System.Text;

namespace umbraco.interfaces
{
    public interface IDataWithPreview : IData
    {
        /// <summary>
        /// Gets or sets a value indicating whether preview mode is switched on.
        /// In preview mode, the <see cref="Value"/> setter saves to a temporary location
        /// instead of persistent storage, which the getter also reads from on subsequent access.
        /// Switching off preview mode restores the persistent value.
        /// </summary>
        /// <value><c>true</c> if preview mode is switched on; otherwise, <c>false</c>.</value>
        bool PreviewMode { get; set; }
    }
}
