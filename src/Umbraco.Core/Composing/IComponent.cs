using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Represents a component.
    /// </summary>
    /// <remarks>
    /// <para>Components are created by DI and therefore must have a public constructor.</para>
    /// <para>All components are terminated in reverse order when Umbraco terminates, and
    /// disposable components are disposed.</para>
    /// <para>The Dispose method may be invoked more than once, and components
    /// should ensure they support this.</para>
    /// </remarks>
    public interface IComponent
    {
        /// <summary>
        /// Initializes the component.
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Terminates the component.
        /// </summary>
        Task TerminateAsync(CancellationToken cancellationToken);
    }
}
