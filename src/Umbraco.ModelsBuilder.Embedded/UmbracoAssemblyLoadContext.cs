using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Umbraco.ModelsBuilder.Embedded
{
    class UmbracoAssemblyLoadContext : AssemblyLoadContext
    {

        public UmbracoAssemblyLoadContext() : base(isCollectible: true)
        {

        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
