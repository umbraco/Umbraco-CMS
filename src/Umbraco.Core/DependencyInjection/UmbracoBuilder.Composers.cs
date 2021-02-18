using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/>
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Umbraco composers for plugins
        /// </summary>
        public static IUmbracoBuilder AddComposers(this IUmbracoBuilder builder)
        {
            // TODO: Should have a better name

            IEnumerable<Type> composerTypes = builder.TypeLoader.GetTypes<IComposer>();
            IEnumerable<Attribute> enableDisable = builder.TypeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));
            new Composers(builder, composerTypes, enableDisable, builder.BuilderLoggerFactory.CreateLogger<Composers>()).Compose();

            return builder;
        }
    }
}
