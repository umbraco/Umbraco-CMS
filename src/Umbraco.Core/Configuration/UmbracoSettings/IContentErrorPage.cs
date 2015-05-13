using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IContentErrorPage
    {
        int ContentId { get; }
        Guid ContentKey { get; }
        string ContentXPath { get; }
        bool HasContentId { get; }
        bool HasContentKey { get; }
        string Culture { get; set; }
    }
}