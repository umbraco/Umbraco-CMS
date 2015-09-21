using System;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a cached content with a GUID key.
    /// </summary>
    /// <remarks>This is temporary, because we cannot add the Key property to IPublishedContent without
    /// breaking backward compatibility. With v8, it will be merged into IPublishedContent.</remarks>
    public interface IPublishedContentWithKey : IPublishedContent
    {
        Guid Key { get; }
    }
}
