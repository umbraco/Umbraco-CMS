using System;
using System.Configuration;
using System.Reflection;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Core.Composing
{
    public class ContainerFactory
    {
        /// <summary>
        /// Creates a new instance of the configured container.
        /// To override the default LightInjectContainer, add an appSetting named umbracoContainerType with
        /// a fully qualified type name to a class with a static method "Create" returning an IContainer.
        /// </summary>
        public static IContainer Create()
        {
            var configuredTypeName = ConfigurationManager.AppSettings["umbracoContainerType"]
                                  ?? typeof(LightInjectContainer).AssemblyQualifiedName;
            var type = Type.GetType(configuredTypeName);
            if (type == null)
            {
                throw new Exception($"Cannot find container factory class named '${configuredTypeName}'");
            }
            var factoryMethod = type.GetMethod("Create", BindingFlags.Static);
            if (factoryMethod == null)
            {
                throw new Exception($"Container factory class '${configuredTypeName}' does not have a public static method named Create");
            }
            var container = factoryMethod.Invoke(null, new object[0]) as IContainer;
            if (container == null)
            {
                throw new Exception($"Container factory '${configuredTypeName}' did not return an IContainer implementation.");
            }
            return container;
        }
    }
}
