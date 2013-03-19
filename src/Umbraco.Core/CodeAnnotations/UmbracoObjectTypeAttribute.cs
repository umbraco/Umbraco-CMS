using System;

namespace Umbraco.Core.CodeAnnotations
{
    /// <summary>
    /// Attribute to associate a GUID string and Type with an UmbracoObjectType Enum value
    /// </summary>
    internal class UmbracoObjectTypeAttribute : Attribute
    {
        public UmbracoObjectTypeAttribute(string objectId)
        {
            ObjectId = new Guid(objectId);
        }

        public UmbracoObjectTypeAttribute(string objectId, Type modelType)
        {
            ObjectId = new Guid(objectId);
            ModelType = modelType;
        }

        public Guid ObjectId { get; private set; }

        public Type ModelType { get; private set; }
    }
}