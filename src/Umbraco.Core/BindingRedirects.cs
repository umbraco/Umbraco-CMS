using System;
using System.Reflection;
using System.Web;
using Umbraco.Core;

// no binding redirect for now = de-activate
//[assembly: PreApplicationStartMethod(typeof(BindingRedirects), "Initialize")]

namespace Umbraco.Core
{
    /// <summary>
    /// Manages any assembly binding redirects that cannot be done via config (i.e. unsigned --> signed assemblies)
    /// </summary>
    public sealed class BindingRedirects
    {
        public static void Initialize()
        {
            // this only gets called when an assembly can't be resolved
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        /// <summary>
        /// This is used to do an assembly binding redirect via code - normally required due to signature changes in assemblies
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // When an assembly can't be resolved. In here we can do magic with the assembly name and try loading another.

            // keep here for reference - we don't use AutoMapper
            /*
            //AutoMapper:
            // this is used for loading a signed assembly of AutoMapper (v. 3.1+) without having to recompile old code.
            // ensure the assembly is indeed AutoMapper and that the PublicKeyToken is null before trying to Load again
            // do NOT just replace this with 'return Assembly', as it will cause an infinite loop -> stackoverflow
            if (args.Name.StartsWith("AutoMapper") && args.Name.EndsWith("PublicKeyToken=null"))
                return Assembly.Load(args.Name.Replace(", PublicKeyToken=null", ", PublicKeyToken=be96cd2c38ef1005"));
            */

            return null;
        }
    }
}
