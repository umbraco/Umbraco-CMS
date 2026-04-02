using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder.Options;

/// <summary>
/// Provides configuration options for property settings used by the ModelsBuilder in Umbraco.
/// </summary>
public class ConfigurePropertySettingsOptions :
    IConfigureOptions<ContentPropertySettings>,
    IConfigureOptions<MemberPropertySettings>,
    IConfigureOptions<MediaPropertySettings>
{
    /// <summary>
    /// Configures the specified <see cref="ContentPropertySettings"/> instance by adding reserved field names.
    /// The reserved names are derived from the property and method names of <see cref="IPublishedContent"/>,
    /// ensuring that these names cannot be used for custom content properties.
    /// </summary>
    /// <param name="options">The <see cref="ContentPropertySettings"/> instance to configure.</param>
    /// <remarks>This helps prevent naming conflicts between custom content properties and the core Umbraco API.</remarks>
    public void Configure(ContentPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    /// <summary>
    /// Configures the specified <see cref="ContentPropertySettings"/> by adding reserved field names derived from the property and method names of <see cref="IPublishedContent"/>.
    /// </summary>
    /// <param name="options">The <see cref="ContentPropertySettings"/> instance to configure.</param>
    public void Configure(MemberPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedMember).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedMember).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }

    /// <summary>
    /// Configures the specified <see cref="ContentPropertySettings" /> by adding reserved field names derived from the property and method names of <see cref="IPublishedContent" />.
    /// </summary>
    /// <param name="options">The <see cref="ContentPropertySettings" /> instance to configure.</param>
    public void Configure(MediaPropertySettings options)
    {
        var reservedProperties = typeof(IPublishedContent).GetPublicProperties().Select(x => x.Name).ToHashSet();
        var reservedMethods = typeof(IPublishedContent).GetPublicMethods().Select(x => x.Name).ToHashSet();
        options.AddReservedFieldNames(reservedProperties);
        options.AddReservedFieldNames(reservedMethods);
    }
}
