using System;

namespace Umbraco.ModelsBuilder
{
    // NOTE
    // that attribute should inherit from PublishedModelAttribute
    // so we do not have different syntaxes
    // but... is sealed at the moment.

    /// <summary>
    /// Indicates that a (partial) class defines the model type for a specific alias.
    /// </summary>
    /// <remarks>Though a model will be generated - so that is the way to register a rename.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ImplementContentTypeAttribute : Attribute
    {
        public ImplementContentTypeAttribute(string alias)
        { }
    }
}
