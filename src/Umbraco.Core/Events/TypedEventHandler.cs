namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents a typed event handler delegate with a specific sender and event arguments type.
/// </summary>
/// <typeparam name="TSender">The type of the event sender.</typeparam>
/// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event arguments.</param>
[Serializable]
public delegate void TypedEventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs e);
