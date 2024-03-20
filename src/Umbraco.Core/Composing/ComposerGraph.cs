using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

// note: this class is NOT thread-safe in any way

/// <summary>
///     Handles the composers.
/// </summary>
internal class ComposerGraph : ComposerGraph<IComposer>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ComposerGraph" /> class.
    /// </summary>
    /// <param name="builder">The composition.</param>
    /// <param name="composerTypes">The <see cref="IComposer" /> types.</param>
    /// <param name="enableDisableAttributes">
    ///     The <see cref="EnableComposerAttribute" /> and/or
    ///     <see cref="DisableComposerAttribute" /> attributes.
    /// </param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">
    ///     composition
    ///     or
    ///     composerTypes
    ///     or
    ///     enableDisableAttributes
    ///     or
    ///     logger
    /// </exception>
    public ComposerGraph(IUmbracoBuilder builder, IEnumerable<Type> composerTypes, IEnumerable<Attribute> enableDisableAttributes, ILogger<ComposerGraph> logger)
        : base(builder, composerTypes, enableDisableAttributes, logger)
    {
    }
}
