using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Wrapper class for OEmbed response with width and height as string values.
/// </summary>
[DataContract]
public class OEmbedResponseWithStringDimensions : OEmbedResponseBase<string>;
