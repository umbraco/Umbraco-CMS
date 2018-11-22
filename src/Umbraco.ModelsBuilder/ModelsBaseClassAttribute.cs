using System;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates the default base class for models.
    /// </summary>
    /// <remarks>Otherwise it is PublishedContentModel. Would make sense to inherit from PublishedContentModel.</remarks>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class ModelsBaseClassAttribute : Attribute
    {
        public ModelsBaseClassAttribute(Type type)
        {}
    }
}

