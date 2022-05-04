using Umbraco.Cms.Core.Events;

// ReSharper disable once CheckNamespace
namespace Umbraco.Cms.Core.Scoping;

[Obsolete("Please use Umbraco.Cms.Infrastructure.Scoping.IScope or Umbraco.Cms.Core.Scoping.ICoreScope instead.")]
public interface IScope : Infrastructure.Scoping.IScope
{
    /// <summary>
    ///     Gets the scope event messages.
    /// </summary>
    EventMessages Messages { get; }

    /// <summary>
    ///     Gets the scope event dispatcher.
    /// </summary>
    IEventDispatcher Events { get; }
}
