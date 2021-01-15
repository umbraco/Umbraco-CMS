using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Umbraco.ModelsBuilder.Embedded
{
    internal class UmbracoAssemblyLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        /// <summary>
        /// Collectible AssemblyLoadContext used to load in the compiled generated models.
        /// Must be a collectible assembly in order to be able to be unloaded.
        /// </summary>
        public UmbracoAssemblyLoadContext() : base(isCollectible: true)
        {
            //_resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            //string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            //if (assemblyPath != null)
            //{
            //    return LoadFromAssemblyPath(assemblyPath);
            //}

            return null;
        }
    }
}
