using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Media object
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Media : ContentBase, IMedia
{
    /// <summary>
    ///     Constructor for creating a Media object
    /// </summary>
    /// <param name="name">name of the Media object</param>
    /// <param name="parent">Parent <see cref="IMedia" /> object</param>
    /// <param name="mediaType">MediaType for the current Media object</param>
    public Media(string? name, IMedia? parent, IMediaType mediaType)
        : this(name, parent, mediaType, new PropertyCollection())
    {
    }

    /// <summary>
    ///     Constructor for creating a Media object
    /// </summary>
    /// <param name="name">name of the Media object</param>
    /// <param name="parent">Parent <see cref="IMedia" /> object</param>
    /// <param name="mediaType">MediaType for the current Media object</param>
    /// <param name="properties">Collection of properties</param>
    public Media(string? name, IMedia? parent, IMediaType mediaType, IPropertyCollection properties)
        : base(name, parent, mediaType, properties)
    {
    }

    /// <summary>
    ///     Constructor for creating a Media object
    /// </summary>
    /// <param name="name">name of the Media object</param>
    /// <param name="parentId">Id of the Parent IMedia</param>
    /// <param name="mediaType">MediaType for the current Media object</param>
    public Media(string? name, int parentId, IMediaType? mediaType)
        : this(name, parentId, mediaType, new PropertyCollection())
    {
    }

    /// <summary>
    ///     Constructor for creating a Media object
    /// </summary>
    /// <param name="name">Name of the Media object</param>
    /// <param name="parentId">Id of the Parent IMedia</param>
    /// <param name="mediaType">MediaType for the current Media object</param>
    /// <param name="properties">Collection of properties</param>
    public Media(string? name, int parentId, IMediaType? mediaType, IPropertyCollection properties)
        : base(name, parentId, mediaType, properties)
    {
    }

    /// <summary>
    ///     Changes the <see cref="IMediaType" /> for the current Media object
    /// </summary>
    /// <param name="contentType">New MediaType for this Media</param>
    /// <remarks>Leaves PropertyTypes intact after change</remarks>
    internal void ChangeContentType(IMediaType mediaType) => ChangeContentType(mediaType, false);

    /// <summary>
    ///     Changes the <see cref="IMediaType" /> for the current Media object and removes PropertyTypes,
    ///     which are not part of the new MediaType.
    /// </summary>
    /// <param name="contentType">New MediaType for this Media</param>
    /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
    internal void ChangeContentType(IMediaType mediaType, bool clearProperties)
    {
        ChangeContentType(new SimpleContentType(mediaType));

        if (clearProperties)
        {
            Properties.EnsureCleanPropertyTypes(mediaType.CompositionPropertyTypes);
        }
        else
        {
            Properties.EnsurePropertyTypes(mediaType.CompositionPropertyTypes);
        }

        Properties.ClearCollectionChangedEvents(); // be sure not to double add
        Properties.CollectionChanged += PropertiesChanged;
    }
}
