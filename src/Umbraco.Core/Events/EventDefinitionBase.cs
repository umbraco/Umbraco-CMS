using System.Reflection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Provides a base class for event definitions that can be tracked and raised by an event dispatcher.
/// </summary>
public abstract class EventDefinitionBase : IEventDefinition, IEquatable<EventDefinitionBase>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventDefinitionBase" /> class.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="eventName">The optional name of the event.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sender" /> or <paramref name="args" /> is null.</exception>
    /// <exception cref="AmbiguousMatchException">Thrown when the event name cannot be automatically determined.</exception>
    protected EventDefinitionBase(object? sender, object? args, string? eventName = null)
    {
        Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        Args = args ?? throw new ArgumentNullException(nameof(args));
        EventName = eventName;

        if (EventName.IsNullOrWhiteSpace())
        {
            // don't match "Ing" suffixed names
            Attempt<EventNameExtractorResult?> findResult =
                EventNameExtractor.FindEvent(sender, args, EventNameExtractor.MatchIngNames);

            if (findResult.Success == false)
            {
                throw new AmbiguousMatchException(
                    "Could not automatically find the event name, the event name will need to be explicitly registered for this event definition. "
                    + $"Sender: {sender.GetType()} Args: {args.GetType()}"
                    + " Error: " + findResult.Result?.Error);
            }

            EventName = findResult.Result?.Name;
        }
    }

    /// <inheritdoc />
    public object Sender { get; }

    /// <inheritdoc />
    public bool Equals(EventDefinitionBase? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Args.Equals(other.Args) && string.Equals(EventName, other.EventName) && Sender.Equals(other.Sender);
    }

    /// <inheritdoc />
    public object Args { get; }

    /// <inheritdoc />
    public string? EventName { get; }

    /// <summary>
    ///     Determines whether two <see cref="EventDefinitionBase" /> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(EventDefinitionBase left, EventDefinitionBase right) => Equals(left, right);

    /// <inheritdoc />
    public abstract void RaiseEvent();

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((EventDefinitionBase)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Args.GetHashCode();
            if (EventName is not null)
            {
                hashCode = (hashCode * 397) ^ EventName.GetHashCode();
            }

            hashCode = (hashCode * 397) ^ Sender.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    ///     Determines whether two <see cref="EventDefinitionBase" /> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(EventDefinitionBase left, EventDefinitionBase right) => Equals(left, right) == false;
}
