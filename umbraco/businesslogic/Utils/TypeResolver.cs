using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

namespace umbraco.BusinessLogic.Utils
{
    /// <summary>
    /// Represents the Umbraco Typeresolver class.
    /// The typeresolver is a collection of utillities for finding and determining types and classes with reflection.
    /// </summary>
	[Serializable]  
	public class TypeResolver : MarshalByRefObject
	{
        /// <summary>
        /// Gets a collection of assignables of type T from a collection of a specific file type from a specified path.
        /// </summary>
        /// <typeparam name="T">The Type</typeparam>
        /// <param name="path">The path.</param>
        /// <param name="filePattern">The file pattern.</param>
        /// <returns></returns>
		public static string[] GetAssignablesFromType<T>(string path, string filePattern)
		{
			FileInfo[] fis = Array.ConvertAll<string, FileInfo>(
				Directory.GetFiles(path, filePattern),
				delegate(string s) { return new FileInfo(s); });
			string[] absoluteFiles = Array.ConvertAll<FileInfo, string>(
				fis, delegate(FileInfo fi) { return fi.FullName; });
			return GetAssignablesFromType<T>(absoluteFiles);
		}

        /// <summary>
        /// Gets a collection of assignables of type T from a collection of files
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="files">The files.</param>
        /// <returns></returns>
		public static string[] GetAssignablesFromType<T>(string[] files)
		{

            AppDomain sandbox = AppDomain.CurrentDomain;

            if ((!GlobalSettings.UseMediumTrust) && (GlobalSettings.ApplicationTrustLevel > AspNetHostingPermissionLevel.Medium)) {
                AppDomainSetup domainSetup = new AppDomainSetup();
                domainSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                domainSetup.ApplicationName = "Umbraco_Sandbox_" + Guid.NewGuid();
                domainSetup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

                domainSetup.DynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase;
                domainSetup.LicenseFile = AppDomain.CurrentDomain.SetupInformation.LicenseFile;
                domainSetup.LoaderOptimization = AppDomain.CurrentDomain.SetupInformation.LoaderOptimization;
                domainSetup.PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
                domainSetup.PrivateBinPathProbe = AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe;
                domainSetup.ShadowCopyFiles = "false";

                sandbox = AppDomain.CreateDomain("Sandbox", AppDomain.CurrentDomain.Evidence, domainSetup);
            }

            try
            {
                TypeResolver typeResolver = (TypeResolver)sandbox.CreateInstanceAndUnwrap(
                    typeof(TypeResolver).Assembly.GetName().Name,
                    typeof(TypeResolver).FullName);

                return typeResolver.GetTypes(typeof(T), files);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                if ((!GlobalSettings.UseMediumTrust) && (GlobalSettings.ApplicationTrustLevel > AspNetHostingPermissionLevel.Medium))
                {
                    AppDomain.Unload(sandbox);
                }
            }

			return new string[0];
		}

        /// <summary>
        /// Returns of a collection of type names from a collection of assembky files.
        /// </summary>
        /// <param name="assignTypeFrom">The assign type.</param>
        /// <param name="assemblyFiles">The assembly files.</param>
        /// <returns></returns>
		public string[] GetTypes(Type assignTypeFrom, string[] assemblyFiles)
		{
			List<string> result = new List<string>();
			foreach(string fileName in assemblyFiles)
			{
				if(!File.Exists(fileName))
					continue;
				try
				{
					Assembly assembly = Assembly.LoadFile(fileName);

                    if (assembly != null) {
                        foreach (Type t in assembly.GetTypes()) {
                            if (!t.IsInterface && assignTypeFrom.IsAssignableFrom(t))
                                result.Add(t.AssemblyQualifiedName);
                        }
                    }
				}
				catch(Exception e)
				{
					Debug.WriteLine(string.Format("Error loading assembly: {0}\n{1}", fileName, e));
			    }
			}

            /*
            try{
                System.Collections.IList list = System.Web.Compilation.BuildManager.CodeAssemblies;
                if (list != null && list.Count > 0) {
                    Assembly asm;
                    foreach (object o in list) {
                        asm = o as Assembly;

                        Log.Add(LogTypes.Debug, -1, "assembly " + asm.ToString()  );
                        if (asm != null) {
                            foreach (Type t in asm.GetTypes()) {
                                if (!t.IsInterface && assignTypeFrom.IsAssignableFrom(t))
                                    result.Add(t.AssemblyQualifiedName);
                            }
                        }
                    }
                } else {
                    Log.Add(LogTypes.Debug, -1, "No assemblies");
                }
            } catch(Exception ee) {
                Log.Add(LogTypes.Debug, -1, ee.ToString());
            }
            */

			return result.ToArray();
		}
	}
}
