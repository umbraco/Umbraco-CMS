using Umbraco.Cms.Core.Events;

namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// This is the format when communicating notification messages to the API consumers.
/// </summary>
/// <remarks>
/// The class is made public on purpose, to make it clear that changing it might constitute a breaking change towards API consumers.
/// </remarks>
public sealed class NotificationHeaderModel
{
    public required string Message { get; init; }

    public required string Category { get; init; }

    public required EventMessageType Type { get; init; }
}
