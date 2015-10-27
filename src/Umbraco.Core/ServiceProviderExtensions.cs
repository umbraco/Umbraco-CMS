using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Core
{
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Used to create instances of the specified type based on the resolved/cached plugin types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider"></param>
        /// <param name="types"></param>
        /// <param name="logger"></param>
        /// <param name="throwException">set to true if an exception is to be thrown if there is an error during instantiation</param>
        /// <returns></returns>
        public static IEnumerable<T> CreateInstances<T>(this IServiceProvider serviceProvider, IEnumerable<Type> types, ILogger logger, bool throwException = false)
        {
            var typesAsArray = types.ToArray();

            var instances = new List<T>();
            foreach (var t in typesAsArray)
            {
                try
                {
                    var typeInstance = (T) serviceProvider.GetService(t);
                    instances.Add(typeInstance);
                }
                catch (Exception ex)
                {

                    logger.Error<PluginManager>(String.Format("Error creating type {0}", t.FullName), ex);

                    if (throwException)
                    {
                        throw;
                    }
                }
            }
            return instances;
        }
    }
}