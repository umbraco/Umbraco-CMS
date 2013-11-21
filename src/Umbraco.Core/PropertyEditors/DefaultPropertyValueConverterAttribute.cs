using System;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Indicates that this is a default property value converter (shipped with Umbraco)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class DefaultPropertyValueConverterAttribute : Attribute
    {
    }
}