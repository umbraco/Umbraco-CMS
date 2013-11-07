using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Xml.XPath
{
    /// <summary>
    /// Represents the type of a content that can be navigated via XPath.
    /// </summary>
    interface INavigableContentType
    {
        /// <summary>
        /// Gets the name of the content type.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the field types of the content type.
        /// </summary>
        /// <remarks>This includes the attributes and the properties.</remarks>
        INavigableFieldType[] FieldTypes { get; }
    }
}
