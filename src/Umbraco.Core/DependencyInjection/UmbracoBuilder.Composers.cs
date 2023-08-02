using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" />
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco composers for plugins
    /// </summary>
    public static IUmbracoBuilder AddComposers(this IUmbracoBuilder builder)
    {
        IEnumerable<Type> composerTypes = builder.TypeLoader.GetTypes<IComposer>();
        IEnumerable<Attribute> enableDisable =
            builder.TypeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));

        new ComposerGraph(builder, composerTypes, enableDisable, builder.BuilderLoggerFactory.CreateLogger<ComposerGraph>()).Compose();

        return builder;
    }

    /// <summary>
    ///     Adds custom composers
    /// </summary>
    public static IUmbracoBuilder AddComposers<TComposer>(this IUmbracoBuilder builder
        where TComposer : ICustomComposer
    {
        IEnumerable<Type> composerTypes = builder.TypeLoader.GetTypes<TComposer>();
        IEnumerable<Attribute> enableDisable =
            builder.TypeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));

        new ComposerGraph<TComposer>(builder, composerTypes, enableDisable, builder.BuilderLoggerFactory.CreateLogger<ComposerGraph<TComposer>>()).Compose();

        return builder;
    }
}
