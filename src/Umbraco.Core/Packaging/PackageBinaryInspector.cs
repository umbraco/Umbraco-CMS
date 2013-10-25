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
        public IEnumerable<string> PerformScan<T>(string dllPath, out string[] errorReport)
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
                var a = Assembly.ReflectionOnlyLoad(e.Name);
                if (a == null) throw new TypeLoadException("Could not load assembly " + e.Name);
                return a;
            };

            //First load each dll file into the context            
            foreach (var f in files) Assembly.ReflectionOnlyLoadFrom(f);

            ////before we try to load these assemblies into context, ensure we haven't done that already
            //var alreadyLoaded = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
            ////First load each dll file into the context            
            //foreach (var f in files.Where(a => !IsAlreadyLoaded(a, alreadyLoaded)))
            //{
            //    //NOTE: if you're loading an already loaded assembly from a new location
            //    // you will get a FileLoadException here.
            //    Assembly.ReflectionOnlyLoadFrom(f);
            //}

            //Then load each referenced assembly into the context
            foreach (var f in files)
            {
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);
                foreach (var assemblyName in reflectedAssembly.GetReferencedAssemblies())
                {
                    try
                    {
                        Assembly.ReflectionOnlyLoad(assemblyName.FullName);
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

            //now that we have all referenced types into the context we can look up stuff
            foreach (var f in files.Except(assembliesWithErrors))
            {
                //now we need to see if they contain any type 'T'
                var reflectedAssembly = Assembly.ReflectionOnlyLoadFrom(f);
                var found = reflectedAssembly.GetExportedTypes().Where(TypeHelper.IsTypeAssignableFrom<T>);
                if (found.Any())
                {
                    dllsWithReference.Add(reflectedAssembly.FullName);
                }
            }

            errorReport = errors.ToArray();
            return dllsWithReference;
        }

        public static IEnumerable<string> ScanAssembliesForTypeReference<T>(string dllPath, out string[] errorReport)
        {
            var appDomain = GetTempAppDomain();
            var type = typeof(PackageBinaryInspector);
            var value = (PackageBinaryInspector) appDomain.CreateInstanceAndUnwrap(
                type.Assembly.FullName, 
                type.FullName);
            var result = value.PerformScan<T>(dllPath, out errorReport);
            AppDomain.Unload(appDomain);
            return result;
        }

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

        //private static bool IsAlreadyLoaded(string assemblyFile, IEnumerable<Assembly> alreadyLoaded)
        //{
        //    return alreadyLoaded.Any(assembly => GetAssemblyLocation(assembly) == assemblyFile);
        //}

        //private static string GetAssemblyLocation(Assembly assembly)
        //{
        //    var uri = new Uri(assembly.CodeBase);
        //    return uri.LocalPath;
        //}

    }
}
