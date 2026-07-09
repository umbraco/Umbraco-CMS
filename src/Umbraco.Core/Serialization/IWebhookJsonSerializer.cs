namespace Umbraco.Cms.Core.Serialization;

/// <summary>
///     Defines a JSON serializer specifically for webhook payloads.
/// </summary>
/// <remarks>
///     This interface extends <see cref="IJsonSerializer" /> to provide specialized
///     serialization for webhook data with appropriate formatting and options.
/// </remarks>
public interface IWebhookJsonSerializer : IJsonSerializer
{ }
