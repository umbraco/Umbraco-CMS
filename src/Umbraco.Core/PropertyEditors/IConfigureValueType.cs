using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a configuration that configures the value type.
/// </summary>
/// <remarks>
///     <para>This is used in <see cref="DataType" /> to get the value type from the configuration.</para>
/// </remarks>
[Obsolete("No longer used by any core editor: a property editor declares its value type on its DataEditorAttribute, so the type a property yields does not depend on the configuration of the data type it is used through. Scheduled for removal in Umbraco 21.")]
public interface IConfigureValueType
{
    /// <summary>
    ///     Gets the value type.
    /// </summary>
    string ValueType { get; }
}
