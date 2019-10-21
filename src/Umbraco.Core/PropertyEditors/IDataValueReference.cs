using System.Collections.Generic;

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
        IEnumerable<Udi> GetReferences(object value);
    }
}
