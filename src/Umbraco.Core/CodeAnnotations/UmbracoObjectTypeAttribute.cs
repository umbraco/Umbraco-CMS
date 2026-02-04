namespace Umbraco.Cms.Core.CodeAnnotations;

/// <summary>
/// Attribute to associate a GUID string and Type with an UmbracoObjectType enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class UmbracoObjectTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoObjectTypeAttribute"/> class with a GUID string.
    /// </summary>
    /// <param name="objectId">The GUID string representing the object type identifier.</param>
    public UmbracoObjectTypeAttribute(string objectId) => ObjectId = new Guid(objectId);

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbracoObjectTypeAttribute"/> class with a GUID string and model type.
    /// </summary>
    /// <param name="objectId">The GUID string representing the object type identifier.</param>
    /// <param name="modelType">The CLR type associated with this object type.</param>
    public UmbracoObjectTypeAttribute(string objectId, Type modelType)
    {
        ObjectId = new Guid(objectId);
        ModelType = modelType;
    }

    /// <summary>
    /// Gets the unique identifier for the Umbraco object type.
    /// </summary>
    public Guid ObjectId { get; }

    /// <summary>
    /// Gets the CLR type associated with this object type, if specified.
    /// </summary>
    public Type? ModelType { get; }
}
