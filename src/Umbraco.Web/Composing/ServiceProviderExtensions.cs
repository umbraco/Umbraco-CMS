using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Web.Composing
{
    public static class ServiceProviderExtensions
    {
        public static object GetInstance(this IServiceProvider container, Type type)
        {
            return container.GetRequiredService(type);
        }

        public static TService GetInstance<TService>(this IServiceProvider container)
        {
            return container.GetRequiredService<TService>();
        }

        public static TService TryGetInstance<TService>(this IServiceProvider container)
            where TService : class 
        {
            try
            {
                return container.GetService<TService>();
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public static object TryGetInstance(this IServiceProvider container, Type type)
        {
            return container.GetService(type);
        }

    }
}
