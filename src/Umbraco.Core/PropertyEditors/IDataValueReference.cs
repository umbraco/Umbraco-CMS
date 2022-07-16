using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Resolve references from <see cref="IDataValueEditor" /> values
/// </summary>
public interface IDataValueReference
{
    /// <summary>
    ///     Returns any references contained in the value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    IEnumerable<UmbracoEntityReference> GetReferences(object? value);
}
