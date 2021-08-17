using System;
using System.Collections.Generic;
using System.Reflection;
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
            builder.WithCollectionBuilder<ComponentCollectionBuilder>();

            IEnumerable<Type> composerTypes = builder.TypeLoader.GetTypes<IComposer>();
            IEnumerable<IComposer> composers = GetComposers(composerTypes);

            foreach (IComposer composer in composers)
            {
                composer.Compose(builder);
            }

            return builder;
        }

        private static IEnumerable<IComposer> GetComposers(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                ConstructorInfo ctor = type.GetConstructor(Array.Empty<Type>());

                if (ctor == null)
                {
                    throw new InvalidOperationException($"Composer {type.FullName} does not have a parameter-less constructor.");
                }

                yield return (IComposer) ctor.Invoke(Array.Empty<object>());
            }
        }
    }
}
