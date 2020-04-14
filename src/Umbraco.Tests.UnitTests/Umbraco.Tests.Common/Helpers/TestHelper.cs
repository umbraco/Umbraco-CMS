using System;
using System.IO;

namespace Umbraco.Tests.UnitTests.Umbraco.Tests.Common.Helpers
{
    /// <summary>
    /// Common helper properties and methods useful to testing
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Some test files are copied to the /bin (/bin/debug) on build, this is a utility to return their physical path based on a virtual path name
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string MapPathForTestFiles(string relativePath)
        {
            if (!relativePath.StartsWith("~/"))
                throw new ArgumentException("relativePath must start with '~/'", nameof(relativePath));

            var codeBase = typeof(TestHelper).Assembly.CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var bin = Path.GetDirectoryName(path);

            return relativePath.Replace("~/", bin + "/");
        }
    }
}
