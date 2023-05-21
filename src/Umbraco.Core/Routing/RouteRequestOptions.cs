namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Options for routing an Umbraco request
/// </summary>
public struct RouteRequestOptions : IEquatable<RouteRequestOptions>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RouteRequestOptions" /> struct.
    /// </summary>
    public RouteRequestOptions(RouteDirection direction) => RouteDirection = direction;

    /// <summary>
    ///     Gets the <see cref="RouteDirection" />
    /// </summary>
    public RouteDirection RouteDirection { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RouteRequestOptions options && Equals(options);

    /// <inheritdoc />
    public bool Equals(RouteRequestOptions other) => RouteDirection == other.RouteDirection;

    /// <inheritdoc />
    public override int GetHashCode() => 15391035 + RouteDirection.GetHashCode();
}
