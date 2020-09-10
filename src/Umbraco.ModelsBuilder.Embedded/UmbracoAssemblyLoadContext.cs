using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Umbraco.ModelsBuilder.Embedded
{
    class UmbracoAssemblyLoadContext : AssemblyLoadContext
    {

        /// <summary>
        /// Collectible AssemblyLoadContext used to load in the compiled generated models.
        /// Must be a collectible assembly in order to be able to be unloaded.
        /// </summary>
        public UmbracoAssemblyLoadContext() : base(isCollectible: true)
        {

        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
