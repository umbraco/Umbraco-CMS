namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Provides support for additional data.
/// </summary>
/// <remarks>
///     <para>Additional data are transient, not deep-cloned.</para>
/// </remarks>
public interface IHaveAdditionalData
{
    /// <summary>
    ///     Gets additional data for this entity.
    /// </summary>
    /// <remarks>
    ///     Can be empty, but never null. To avoid allocating, do not
    ///     test for emptiness, but use <see cref="HasAdditionalData" /> instead.
    /// </remarks>
    IDictionary<string, object?>? AdditionalData { get; }

    /// <summary>
    ///     Determines whether this entity has additional data.
    /// </summary>
    /// <remarks>
    ///     Use this property to check for additional data without
    ///     getting <see cref="AdditionalData" />, to avoid allocating.
    /// </remarks>
    bool HasAdditionalData { get; }

    // how to implement:

    /*
    private IDictionary<string, object> _additionalData;

    /// <inheritdoc />
    [DataMember]
    [DoNotClone]
    PublicAccessEntry IDictionary<string, object> AdditionalData => _additionalData ?? (_additionalData = new Dictionary<string, object>());

    /// <inheritdoc />
    [IgnoreDataMember]
    PublicAccessEntry bool HasAdditionalData => _additionalData != null;
    */
}
