using System;
using System.IO;
using System.Reflection;
using log4net.Config;

namespace Umbraco.Tests.TestHelpers
{
	/// <summary>
	/// Common helper properties and methods useful to testing
	/// </summary>
	public static class TestHelper
	{
		/// <summary>
		/// Gets the current assembly directory.
		/// </summary>
		/// <value>The assembly directory.</value>
		static public string CurrentAssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetCallingAssembly().CodeBase;
				var uri = new Uri(codeBase);
				var path = uri.LocalPath;
				return Path.GetDirectoryName(path);
			}
		}

		/// <summary>
		/// Maps the given <paramref name="relativePath"/> making it rooted on <see cref="CurrentAssemblyDirectory"/>. <paramref name="relativePath"/> must start with <code>~/</code>
		/// </summary>
		/// <param name="relativePath">The relative path.</param>
		/// <returns></returns>
		public static string MapPathForTest(string relativePath)
		{
			if (!relativePath.StartsWith("~/"))
				throw new ArgumentException("relativePath must start with '~/'", "relativePath");

			return relativePath.Replace("~/", CurrentAssemblyDirectory + "/");
		}

		public static void SetupLog4NetForTests()
		{
			XmlConfigurator.Configure(new FileInfo(MapPathForTest("~/unit-test-log4net.config")));
		}
	}
}