using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using SqlCE4Umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.DataLayer;
using GlobalSettings = umbraco.GlobalSettings;

namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
	/// Common helper properties and methods useful to testing
	/// </summary>
	public static class TestHelper
	{

		/// <summary>
		/// Clears an initialized database
		/// </summary>
		public static void ClearDatabase()
		{
            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;
			
			if (dataHelper == null)
				throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");

			dataHelper.ClearDatabase();
		}

        public static void DropForeignKeys(string table)
        {
            var databaseSettings = ConfigurationManager.ConnectionStrings[Core.Configuration.GlobalSettings.UmbracoConnectionName];
            var dataHelper = DataLayerHelper.CreateSqlHelper(databaseSettings.ConnectionString, false) as SqlCEHelper;

            if (dataHelper == null)
                throw new InvalidOperationException("The sql helper for unit tests must be of type SqlCEHelper, check the ensure the connection string used for this test is set to use SQLCE");

            dataHelper.DropForeignKeys(table);
        }

		/// <summary>
		/// Gets the current assembly directory.
		/// </summary>
		/// <value>The assembly directory.</value>
		static public string CurrentAssemblyDirectory
		{
			get
			{
				var codeBase = typeof(TestHelper).Assembly.CodeBase;
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

	    public static void InitializeContentDirectories()
        {
            CreateDirectories(new[] { SystemDirectories.Masterpages, SystemDirectories.MvcViews, SystemDirectories.Media, SystemDirectories.AppPlugins });
        }

	    public static void CleanContentDirectories()
	    {
	        CleanDirectories(new[] { SystemDirectories.Masterpages, SystemDirectories.MvcViews, SystemDirectories.Media });
	    }

	    public static void CreateDirectories(string[] directories)
        {
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                if (directoryInfo.Exists == false)
                    Directory.CreateDirectory(IOHelper.MapPath(directory));
            }
        }

	    public static void CleanDirectories(string[] directories)
	    {
	        var preserves = new Dictionary<string, string[]>
	        {
	            { SystemDirectories.Masterpages, new[] {"dummy.txt"} },
	            { SystemDirectories.MvcViews, new[] {"dummy.txt"} }
	        };
            foreach (var directory in directories)
            {
                var directoryInfo = new DirectoryInfo(IOHelper.MapPath(directory));
                var preserve = preserves.ContainsKey(directory) ? preserves[directory] : null;
                if (directoryInfo.Exists)
                    directoryInfo.GetFiles().Where(x => preserve == null || preserve.Contains(x.Name) == false).ForEach(x => x.Delete());
            }
        }

	    public static void CleanUmbracoSettingsConfig()
        {
            var currDir = new DirectoryInfo(CurrentAssemblyDirectory);

            var umbracoSettingsFile = Path.Combine(currDir.Parent.Parent.FullName, "config", "umbracoSettings.config");
            if (File.Exists(umbracoSettingsFile))
                File.Delete(umbracoSettingsFile);
        }


	}
}