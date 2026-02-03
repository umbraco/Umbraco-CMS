using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Content object
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Content : PublishableContentBase, IContent
{
    private int? _templateId;

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, string? culture = null)
        : this(name, parent, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="userId">The identifier of the user creating the Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, int userId, string? culture = null)
        : this(name, parent, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, PropertyCollection properties, string? culture = null)
        : base(name, parent, contentType, properties, culture)
    {
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string? name, int parentId, IContentType? contentType, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="userId">The identifier of the user creating the Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, int parentId, IContentType contentType, int userId, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string? name, int parentId, IContentType? contentType, PropertyCollection properties, string? culture = null)
        : base(name, parentId, contentType, properties, culture)
    {
    }

    /// <summary>
    ///     Gets or sets the template used by the Content.
    ///     This is used to override the default one from the ContentType.
    /// </summary>
    /// <remarks>
    ///     If no template is explicitly set on the Content object,
    ///     the Default template from the ContentType will be returned.
    /// </remarks>
    [DataMember]
    public int? TemplateId
    {
        get => _templateId;
        set => SetPropertyValueAndDetectChanges(value, ref _templateId, nameof(TemplateId));
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public int? PublishTemplateId { get; set; } // set by persistence

    [DataMember]
    public bool Blueprint { get; set; }

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity and it's property identities reset
    /// </summary>
    /// <returns></returns>
    public IContent DeepCloneWithResetIdentities()
    {
        var clone = (Content)DeepClone();
        clone.ResetIdentity();
        clone.VersionId = clone.PublishedVersionId = 0;

        foreach (IProperty property in clone.Properties)
        {
            property.ResetIdentity();
        }

        return clone;
    }
}
