using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Resolve tags from <see cref="IDataValueEditor" /> values
/// </summary>
public interface IDataValueTags
{
    /// <summary>
    ///     Returns any tags contained in the value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="tagConfiguration"></param>
    /// <param name="languageId"></param>
    /// <returns></returns>
    IEnumerable<ITag> GetTags(object? value, TagConfiguration? tagConfiguration, int? languageId);
}
