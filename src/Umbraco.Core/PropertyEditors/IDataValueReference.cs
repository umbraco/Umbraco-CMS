using System.Collections.Generic;
using Umbraco.Core.Models.Editors;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Resolve references from <see cref="IDataValueEditor"/> values
    /// </summary>
    public interface IDataValueReference
    {
        /// <summary>
        /// Returns any references contained in the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<UmbracoEntityReference> GetReferences(object value);

        /// <summary>
        /// Gets a value indicating whether the DataValueReference lookup supports a datatype (data editor).
        /// </summary>
        /// <param name="dataType">The datatype.</param>
        /// <returns>A value indicating whether the converter supports a datatype.</returns>
        bool IsForEditor(IDataEditor dataEditor);
    }
}
