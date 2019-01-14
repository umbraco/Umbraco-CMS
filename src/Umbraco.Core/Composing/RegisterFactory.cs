using System;
using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing.MSDI;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Creates the container.
    /// </summary>
    public static class RegisterFactory
    {

        // cannot use typeof().AssemblyQualifiedName on the web container - we don't reference it
        // a normal Umbraco site should run on the web container, but an app may run on the core one
        private const string CoreLightInjectContainerTypeName = "Umbraco.Core.Composing.LightInject.LightInjectContainer,Umbraco.Core";
        private const string WebLightInjectContainerTypeName = "Umbraco.Web.Composing.LightInject.LightInjectContainer,Umbraco.Web";

        private static string _configuredTypeName = ConfigurationManager.AppSettings["umbracoRegisterType"];

        /// <summary>
        /// Creates a new instance of the configured container.
        /// </summary>
        /// <remarks>
        /// To override the default LightInjectContainer, add an appSetting named umbracoContainerType with
        /// a fully qualified type name to a class with a static method "Create" returning an IRegister.
        /// </remarks>
        public static IRegister Create()
        {
            var type = GetFactoryType();

            var factoryMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            if (factoryMethod == null)
                throw new Exception($"Register factory class '{_configuredTypeName}' does not have a public static method named Create.");

            var container = factoryMethod.Invoke(null, Array.Empty<object>()) as IRegister;
            if (container == null)
                throw new Exception($"Register factory '{_configuredTypeName}' did not return an IRegister implementation.");

            return container;
        }

        public static IFactory CreateFactory(IServiceCollection serviceCollection)
        {
            var type = GetFactoryType();

            // TODO: Generalize
            var factoryMethod = type.GetMethod("CreateFactory", BindingFlags.Public | BindingFlags.Static, null, new []{typeof(IServiceCollection)}, null);
            if (factoryMethod == null)
                throw new Exception($"Register factory class '{_configuredTypeName}' does not have a public static method CreateFactory(IServiceCollection services).");

            var container = factoryMethod.Invoke(null, new []{serviceCollection}) as IFactory;
            if (container == null)
                throw new Exception($"Register factory '{_configuredTypeName}' did not return an IFactory implementation.");

            return container;
        }

        private static Type GetFactoryType()
        {
            Type type;
            if (_configuredTypeName.IsNullOrWhiteSpace())
            {
                // try to get the web LightInject container type,
                // else the core LightInject container type
                type = Type.GetType(_configuredTypeName = WebLightInjectContainerTypeName) ??
                       Type.GetType(_configuredTypeName = CoreLightInjectContainerTypeName);
            }
            else
            {
                // try to get the configured type
                type = Type.GetType(_configuredTypeName);
            }

            if (type == null)
                throw new Exception($"Cannot find register factory class '{_configuredTypeName}'.");
            return type;
        }
    }
}
