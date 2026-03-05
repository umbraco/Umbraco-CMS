using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an Element object
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Element : PublishableContentBase, IElement
{
    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string name, IContentType contentType, string? culture = null)
        : this(name, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="userId">The identifier of the user creating the Element object</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string name, IContentType contentType, int userId, string? culture = null)
        : this(name, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string name, IContentType contentType, PropertyCollection properties, string? culture = null)
        : base(name, Constants.System.Root, contentType, properties, culture)
    {
    }


    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="parentId">Id of the Parent folder</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string? name, int parentId, IContentType? contentType, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="parentId">Id of the Parent folder</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="userId">The identifier of the user creating the Element object</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string name, int parentId, IContentType contentType, int userId, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating an Element object
    /// </summary>
    /// <param name="name">Name of the element</param>
    /// <param name="parentId">Id of the Parent folder</param>
    /// <param name="contentType">ContentType for the current Element object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Element(string? name, int parentId, IContentType? contentType, PropertyCollection properties, string? culture = null)
        : base(name, parentId, contentType, properties, culture)
    {
    }

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity and it's property identities reset
    /// </summary>
    /// <returns></returns>
    public IElement DeepCloneWithResetIdentities()
    {
        var clone = (Element)DeepClone();
        clone.Key = Guid.Empty;
        clone.VersionId = clone.PublishedVersionId = 0;
        clone.ResetIdentity();

        foreach (IProperty property in clone.Properties)
        {
            property.ResetIdentity();
        }

        return clone;
    }
}
