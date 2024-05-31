using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Media.EmbedProviders;

/// <summary>
///     Wrapper class for OEmbed response.
/// </summary>
[DataContract]
public class OEmbedResponse : OEmbedResponseBase<double>;

