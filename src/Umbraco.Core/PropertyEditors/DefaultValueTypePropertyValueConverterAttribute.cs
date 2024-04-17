namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Indicates that this is a default value type property value converter (shipped with Umbraco).
///     This attribute is for internal use only. It should never be applied to custom value converters.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DefaultValueTypePropertyValueConverterAttribute : DefaultPropertyValueConverterAttribute
{
}
