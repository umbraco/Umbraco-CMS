using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Provides extensions methods for <see cref="IDataType"/>.
    /// </summary>
    public static class DataTypeExtensions
    {
        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        /// <typeparam name="T">The expected type of the configuration object.</typeparam>
        /// <param name="dataType">This datatype.</param>
        /// <exception cref="InvalidCastException">When the datatype configuration is not of the expected type.</exception>
        public static T ConfigurationAs<T>(this IDataType dataType)
            where T : class
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));

            var configuration = dataType.Configuration;

            switch (configuration)
            {
                case null:
                    return null;
                case T configurationAsT:
                    return configurationAsT;
            }

            throw new InvalidCastException($"Cannot cast dataType configuration, of type {configuration.GetType().Name}, to {typeof(T).Name}.");
        }
    }
}
