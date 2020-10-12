using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Adds all automatically discovered and enabled <see cref="IApplicationComponent" /> types.
    /// </summary>
    /// <seealso cref="Umbraco.Core.Composing.ICoreComposer" />
    public class ApplicationComponentComposer : ICoreComposer
    {
        /// <summary>
        /// Compose all <see cref="IApplicationComponent" /> types.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public void Compose(Composition composition)
        {
            var types = composition.TypeLoader.GetTypes<IApplicationComponent>().ToList();

            var enabledTypes = new Dictionary<Type, EnableInfo>();
            void UpdateEnableInfo(Type type, int weight, bool enabled)
            {
                if (!enabledTypes.TryGetValue(type, out var enableInfo))
                {
                    enableInfo = enabledTypes[type] = new EnableInfo();
                }

                if (enableInfo.Weight > weight)
                {
                    return;
                }

                enableInfo.Enabled = enabled;
                enableInfo.Weight = weight;
            }

            foreach (var attr in composition.TypeLoader.GetAssemblyAttributes<EnableApplicationComponentAttribute>())
            {
                UpdateEnableInfo(attr.EnabledType, 2, true);
            }

            foreach (var attr in composition.TypeLoader.GetAssemblyAttributes<DisableApplicationComponentAttribute>())
            {
                UpdateEnableInfo(attr.DisabledType, 2, false);
            }

            foreach (var type in types)
            {
                foreach (var attr in type.GetCustomAttributes<EnableAttribute>())
                {
                    var enabledType = attr.EnabledType ?? type;
                    var weight = enabledType == type ? 1 : 3;
                    UpdateEnableInfo(enabledType, weight, true);
                }

                foreach (var attr in type.GetCustomAttributes<DisableAttribute>())
                {
                    var disabledType = attr.DisabledType ?? type;
                    var weight = disabledType == type ? 1 : 3;
                    UpdateEnableInfo(disabledType, weight, false);
                }
            }

            foreach (var kvp in enabledTypes.Where(x => !x.Value.Enabled))
            {
                types.Remove(kvp.Key);
            }

            composition.Components().Append(types);
        }

        /// <summary>
        /// Keeps track of types that are enabled at a specific weight.
        /// </summary>
        private class EnableInfo
        {
            /// <summary>
            /// Gets or sets a value indicating whether the type is enabled.
            /// </summary>
            /// <value>
            ///   <c>true</c> if is type is enabled; otherwise, <c>false</c>.
            /// </value>
            public bool Enabled { get; set; }

            /// <summary>
            /// Gets or sets the weight.
            /// </summary>
            /// <value>
            /// The weight.
            /// </value>
            public int Weight { get; set; } = -1;
        }
    }
}
