using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a configuration that configures the value type.
/// </summary>
/// <remarks>
///     <para>This is used in <see cref="DataType" /> to get the value type from the configuration.</para>
/// </remarks>
public interface IConfigureValueType
{
    /// <summary>
    ///     Gets the value type.
    /// </summary>
    string ValueType { get; }
}
