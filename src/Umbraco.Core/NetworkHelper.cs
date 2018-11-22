using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Currently just used to get the machine name in med trust and to format a machine name for use with file names
    /// </summary>
    internal class NetworkHelper
    {
        /// <summary>
        /// Returns the machine name that is safe to use in file paths.
        /// </summary>
        /// <remarks>
        /// see: https://github.com/Shandem/ClientDependency/issues/4
        /// </remarks>
        public static string FileSafeMachineName
        {
            get { return MachineName.ReplaceNonAlphanumericChars('-'); }
        }

        /// <summary>
        /// Returns the current machine name
        /// </summary>
        /// <remarks>
        /// Tries to resolve the machine name, if it cannot it uses the config section.
        /// </remarks>
        public static string MachineName
        {
            get
            {
                try
                {
                    return Environment.MachineName;
                }
                catch
                {
                    try
                    {
                        return System.Net.Dns.GetHostName();
                    }
                    catch
                    {
                        //if we get here it means we cannot access the machine name
                        throw new ApplicationException("Cannot resolve the current machine name eithe by Environment.MachineName or by Dns.GetHostname()");
                    }
                }
            }
        }
    }
}