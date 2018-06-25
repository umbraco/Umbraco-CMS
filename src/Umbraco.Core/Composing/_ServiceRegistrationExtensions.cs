using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing
{
    public static class ServiceRegistrationExtensions
    {
        public static void RegisterFrom<TComposition>(this IServiceCollection services)
            where TComposition : IComposition, new()
        {
            var composition = new TComposition();
            composition.Compose(services);
        }

        /// <summary>
        /// Registers and instanciates a collection builder.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the collection builder.</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>The collection builder.</returns>
        public static void RegisterCollectionBuilder<TBuilder>(this IServiceCollection services)
            where TBuilder: class
        {
            // make sure it's not already registered
            // we just don't want to support re-registering collection builders
            if (services.Any(x => typeof(TBuilder).IsAssignableFrom(x.ServiceType)))
                throw new InvalidOperationException("Collection builders should be registered only once.");

            // register the builder - per container
            services.AddSingleton<TBuilder>();
        }

    }
}
