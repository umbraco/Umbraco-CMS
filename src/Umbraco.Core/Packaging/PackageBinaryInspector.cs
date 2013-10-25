using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Umbraco.Core.Packaging
{
    internal class PackageBinaryInspector : MarshalByRefObject
    {
        /// <summary>
        /// Entry point to call from your code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dllPath"></param>
        /// <param name="errorReport"></param>
        /// <returns></returns>
        /// <remarks>
        /// Will perform the assembly scan in a separate app domain
        /// </remarks>
        public static IEnumerable<string> ScanAssembliesForTypeReference<T>(string dllPath, out string[] errorReport)
        {
            var appDomain = GetTempAppDomain();
            var type = typeof(PackageBinaryInspector);
            try
            {
                var value = (PackageBinaryInspector)appDomain.CreateInstanceAndUnwrap(
                       type.Assembly.FullName,
                       type.FullName);
                var result = value.PerformScan<T>(dllPath, out errorReport);
                return result;
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        /// <summary>
        /// Performs the assembly scanning
        /// </summary>
        /// <typeparam name="T"></typeparam>        
        /// <param name="dllPath"></param>
        /// <param name="errorReport"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is executed in a separate app domain
        /// </remarks>
        internal IEnumerable<string> PerformScan<T>(string dllPath, out string[] errorReport)
        {
            if (Directory.Exists(dllPath) == false)
            {
                throw new DirectoryNotFoundException("Could not find directory " + dllPath);
            }

            var files = Directory.GetFiles(dllPath, "*.dll");
            var dllsWithReference = new List<string>();
            var errors = new List<string>();
            var assembliesWithErrors = new List<string>();

            //we need this handler to resolve assembly dependencies below
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (s, e) =>
                {
                    var a = Assembly.ReflectionOnlyLoadFrom(GetAssemblyPath(Assembly.ReflectionOnlyLoad(e.Name)));                    
                    if (a == null) throw new TypeLoadException("Could not load assembly " + e.Name);
                    return a;
                };

            //First load each dll file into the context            
            foreach (var f in files) Assembly.ReflectionOnlyLoadFrom(f);

            //Then load each referenced assembly into the context
            foreach (var f in files)
            {
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);
                foreach (var assemblyName in reflectedAssembly.GetReferencedAssemblies())
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoadFrom(GetAssemblyPath(Assembly.ReflectionOnlyLoad(assemblyName.FullName)));
                    }
                    catch (FileNotFoundException)
                    {
                        //if an exception occurs it means that a referenced assembly could not be found                        
                        errors.Add(
                            string.Concat("This package references the assembly '",
                                assemblyName.Name,
                                "' which was not found, this package may have problems running"));
                        assembliesWithErrors.Add(f);
                    }
                }
            }

            var contractType = GetLoadFromContractType<T>();

            //now that we have all referenced types into the context we can look up stuff
            foreach (var f in files.Except(assembliesWithErrors))
            {
                //now we need to see if they contain any type 'T'
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);

                var found = reflectedAssembly.GetExportedTypes()
                    .Where(contractType.IsAssignableFrom);

                if (found.Any())
                {
                    dllsWithReference.Add(reflectedAssembly.FullName);
                }
            }

            errorReport = errors.ToArray();
            return dllsWithReference;
        }

        /// <summary>
        /// In order to compare types, the types must be in the same context, this method will return the type that
        /// we are checking against but from the LoadFrom context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Type GetLoadFromContractType<T>()
        {
            var contractAssemblyLoadFrom = Assembly.ReflectionOnlyLoadFrom(
                GetAssemblyPath(Assembly.ReflectionOnlyLoad(typeof (T).Assembly.FullName)));

            var contractType = contractAssemblyLoadFrom.GetExportedTypes()
                .FirstOrDefault(x => x.FullName == typeof(T).FullName && x.Assembly.FullName == typeof(T).Assembly.FullName);
            
            if (contractType == null)
            {
                throw new InvalidOperationException("Could not find type " + typeof(T) + " in the LoadFrom assemblies");
            }
            return contractType;
        }

        /// <summary>
        /// Create an app domain
        /// </summary>
        /// <returns></returns>
        private static AppDomain GetTempAppDomain()
        {
            //copy the current app domain setup but don't shadow copy files
            var appName = "TempDomain" + Guid.NewGuid();
            var domainSetup = new AppDomainSetup
                {
                    ApplicationName = appName,
                    ShadowCopyFiles = "false",
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    DynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase,
                    LicenseFile = AppDomain.CurrentDomain.SetupInformation.LicenseFile,
                    LoaderOptimization = AppDomain.CurrentDomain.SetupInformation.LoaderOptimization,
                    PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath,
                    PrivateBinPathProbe = AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe
                };

            //create new domain with full trust
            return AppDomain.CreateDomain(
                appName,
                AppDomain.CurrentDomain.Evidence,
                domainSetup,
                new PermissionSet(PermissionState.Unrestricted));
        }

        private static string GetAssemblyPath(Assembly a)
        {
            var codeBase = a.CodeBase;
            var uri = new Uri(codeBase);
            return uri.LocalPath;
        }

    }
}
