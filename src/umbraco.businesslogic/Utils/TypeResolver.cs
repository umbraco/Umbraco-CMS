using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Web;

using System.Security;
using System.Security.Permissions;

namespace umbraco.BusinessLogic.Utils
{
    /// <summary>
    /// Represents the Umbraco Typeresolver class.
    /// The typeresolver is a collection of utillities for finding and determining types and classes with reflection.
    /// </summary>
    [Serializable]
	[Obsolete("This class is not longer used and will be removed in future versions")]
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

            if (Umbraco.Core.SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted
                )
            {
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

                PermissionSet trustedLoadFromRemoteSourceGrantSet = new PermissionSet(PermissionState.Unrestricted);
                sandbox = AppDomain.CreateDomain("Sandbox", AppDomain.CurrentDomain.Evidence, domainSetup, trustedLoadFromRemoteSourceGrantSet);
//                sandbox = AppDomain.CreateDomain("Sandbox", AppDomain.CurrentDomain.Evidence, domainSetup);
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
				if (Umbraco.Core.SystemUtilities.GetCurrentTrustLevel() == AspNetHostingPermissionLevel.Unrestricted)
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
            foreach (string fileName in assemblyFiles)
            {
                if (!File.Exists(fileName))
                    continue;
                try
                {
                    Assembly assembly = Assembly.LoadFile(fileName);

                    if (assembly != null)
                    {
                        foreach (Type t in assembly.GetExportedTypes())
                        {
                            if (!t.IsInterface && assignTypeFrom.IsAssignableFrom(t))
                                result.Add(t.AssemblyQualifiedName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(string.Format("Error loading assembly: {0}\n{1}", fileName, e));
                }
            }

            return result.ToArray();
        }

		// zb-00044 #29989 : helper method to help injecting services (poor man's ioc)
		/// <summary>
		/// Creates an instance of the type indicated by <paramref name="fullName"/> and ensures
		/// it implements the interface indicated by <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the service interface.</typeparam>
		/// <param name="service">The name of the service we're trying to implement.</param>
		/// <param name="fullName">The full name of the type implementing the service.</param>
		/// <returns>A class implementing <typeparamref name="T"/>.</returns>
		public static T CreateInstance<T>(string service, string fullName)
			where T : class
		{
			// try to get the assembly and type names
			var parts = fullName.Split(',');
			if (parts.Length != 2)
				throw new Exception(string.Format("Can not create instance for '{0}': '{1}' is not a valid type full name.", service, fullName));

			string typeName = parts[0];
			string assemblyName = parts[1];
			T impl;
			Assembly assembly;
			Type type;

			// try to load the assembly
			try
			{
				assembly = Assembly.Load(assemblyName);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Can not create instance for '{0}', failed to load assembly '{1}': {2} was thrown ({3}).", service, assemblyName, e.GetType().FullName, e.Message));
			}

			// try to get the type
			try
			{
				type = assembly.GetType(typeName);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Can not create instance for '{0}': failed to get type '{1}' in assembly '{2}': {3} was thrown ({4}).", service, typeName, assemblyName, e.GetType().FullName, e.Message));
			}

			if (type == null)
				throw new Exception(string.Format("Can not create instance for '{0}': failed to get type '{1}' in assembly '{2}'.", service, typeName, assemblyName));

			// try to instanciate the type
			try
			{
				impl = Activator.CreateInstance(type) as T;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Can not create instance for '{0}': failed to instanciate: {1} was thrown ({2}).", service, e.GetType().FullName, e.Message));
			}

			// ensure it implements the requested type
			if (impl == null)
				throw new Exception(string.Format("Can not create instance for '{0}': type '{1}' does not implement '{2}'.", service, fullName, typeof(T).FullName));

			return impl;
		}

	}
}
