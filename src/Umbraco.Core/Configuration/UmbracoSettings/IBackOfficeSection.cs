using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IBackOfficeSection
    {
        ITourSection Tours { get; }
        string Id { get; }
    }
}
